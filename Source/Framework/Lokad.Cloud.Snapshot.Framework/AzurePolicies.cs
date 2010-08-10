#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Linq;
using System.Data.Services.Client;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cloud.Snapshot.Framework
{
	// TODO: Consider to reuse the Lokad.Cloud implementation, which is currently internal

	/// <summary>
	/// Azure retry policies for corner-situation and server errors.
	/// </summary>
	internal static class AzurePolicies
	{
		/// <summary>
		/// Retry policy to temporarily back off in case of transient Azure server
		/// errors, system overload or in case the denial of service detection system
		/// thinks we're a too heavy user. Blocks the thread while backing off to
		/// prevent further requests for a while (per thread).
		/// </summary>
		public static ActionPolicy TransientServerErrorBackOff { get; private set; }

		/// <summary>Similar to <see cref="TransientServerErrorBackOff"/>, yet
		/// the Table Storage comes with its own set or exceptions/.</summary>
		public static ActionPolicy TransientTableErrorBackOff { get; private set; }

		/// <summary>
		/// Very patient retry policy to deal with container, queue or table instantiation
		/// that happens just after a deletion.
		/// </summary>
		public static ActionPolicy SlowInstantiation { get; private set; }

		/// <summary>
		/// Static Constructor
		/// </summary>
		static AzurePolicies()
		{
			TransientServerErrorBackOff = ActionPolicy.With(TransientServerErrorExceptionFilter)
				.Retry(30, OnTransientServerErrorRetry);

			TransientTableErrorBackOff = ActionPolicy.With(TransientTableErrorExceptionFilter)
				.Retry(30, OnTransientTableErrorRetry);

			SlowInstantiation = ActionPolicy.With(SlowInstantiationExceptionFilter)
				.Retry(30, OnSlowInstantiationRetry);
		}

		static void OnTransientServerErrorRetry(Exception exception, int count)
		{
			// quadratic backoff, capped at 5 minutes
			var c = count + 1;
			SystemUtil.Sleep(Math.Min(300, c * c).Seconds());
		}

		static void OnTransientTableErrorRetry(Exception exception, int count)
		{
			// quadratic backoff, capped at 5 minutes
			var c = count + 1;
			SystemUtil.Sleep(Math.Min(300, c * c).Seconds());
		}

		static void OnSlowInstantiationRetry(Exception exception, int count)
		{
			// linear backoff
			SystemUtil.Sleep((100 * count).Milliseconds());
		}

		static bool IsErrorCodeMatch(StorageException exception, params StorageErrorCode[] codes)
		{
			return exception != null
				&& codes.Contains(exception.ErrorCode);
		}

		static bool IsErrorStringMatch(StorageException exception, params string[] errorStrings)
		{
			return exception != null
				&& exception.ExtendedErrorInformation != null
				&& errorStrings.Contains(exception.ExtendedErrorInformation.ErrorCode);
		}

		static bool IsErrorStringMatch(Exception dataServiceException, params string[] errorStrings)
		{
			if (dataServiceException == null
				|| dataServiceException.InnerException == null
				|| !(dataServiceException is DataServiceQueryException) && !(dataServiceException is DataServiceRequestException))
			{
				return false;
			}

			var match = Regex.Match(dataServiceException.InnerException.Message, @"<code>(\w+)</code>", RegexOptions.IgnoreCase);
			return match.Success && errorStrings.Contains(match.Groups[1].Value);
		}

		public static string GetErrorCode(Exception exception)
		{
			return Regex.Match(exception.InnerException.Message, @"<code>(\w+)</code>", RegexOptions.IgnoreCase).Groups[1].Value;
		}

		static bool TransientServerErrorExceptionFilter(Exception exception)
		{
			var serverException = exception as StorageServerException;
			if (serverException == null)
			{
				// we only handle server exceptions here
				return false;
			}

			if (IsErrorCodeMatch(serverException,
				StorageErrorCode.ServiceInternalError,
				StorageErrorCode.ServiceTimeout))
			{
				return true;
			}

			if (IsErrorStringMatch(serverException,
				StorageErrorCodeStrings.InternalError,
				StorageErrorCodeStrings.ServerBusy,
				StorageErrorCodeStrings.OperationTimedOut))
			{
				return true;
			}

			return false;
		}

		static bool TransientTableErrorExceptionFilter(Exception exception)
		{
			// HACK: StorageClient does not catch very well, internal errors of the table storage.
			// Hence we end up here manually catching exception that should have been correctly 
			// typed by the StorageClient, such as:
			// The remote server returned an error: (500) Internal Server Error.
			var webException = exception as WebException;
			if (null != webException &&
				(webException.Status == WebExceptionStatus.ProtocolError ||
				 webException.Status == WebExceptionStatus.ConnectionClosed))
			{
				return true;
			}

			if (IsErrorStringMatch(exception,
				StorageErrorCodeStrings.InternalError,
				StorageErrorCodeStrings.ServerBusy,
				StorageErrorCodeStrings.OperationTimedOut,
				TableErrorCodeStrings.TableServerOutOfMemory))
			{
				return true;
			}

			return false;
		}

		static bool SlowInstantiationExceptionFilter(Exception exception)
		{
			var storageException = exception as StorageClientException;

			// Blob Storage or Queue Storage exceptions
			// Table Storage may throw exception of type 'StorageClientException'
			if (storageException != null)
			{
				// 'client' exceptions reflect server-side problems (delayed instantiation)

				if (IsErrorCodeMatch(storageException,
					StorageErrorCode.ResourceNotFound,
					StorageErrorCode.ContainerNotFound))
				{
					return true;
				}

				if (IsErrorStringMatch(storageException,
					QueueErrorCodeStrings.QueueNotFound,
					QueueErrorCodeStrings.QueueBeingDeleted,
					StorageErrorCodeStrings.InternalError,
					StorageErrorCodeStrings.ServerBusy,
					TableErrorCodeStrings.TableServerOutOfMemory,
					TableErrorCodeStrings.TableNotFound,
					TableErrorCodeStrings.TableBeingDeleted))
				{
					return true;
				}
			}

			if (IsErrorStringMatch(exception,
				TableErrorCodeStrings.TableBeingDeleted,
				TableErrorCodeStrings.TableNotFound,
				TableErrorCodeStrings.TableServerOutOfMemory))
			{
				return true;
			}

			return false;
		}
	}
}