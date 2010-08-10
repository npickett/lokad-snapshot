#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cloud.Snapshot.Framework
{
	public static class BlobTransfer
	{
		public static void SnapshotContainer(this CloudClients clients, CloudName container, Func<string, bool> predicate, Maybe<int> count, ContinuationToken continuation)
		{
			CreateBlobContainer(clients.SnapshotBlobs, container.SnapshotName);
			Transfer(
				() => clients.LiveBlobs.GetContainerReference(container.LiveName),
				() => clients.SnapshotBlobs.GetContainerReference(container.SnapshotName),
				predicate, count, continuation);
		}

		public static void RestoreContainer(this CloudClients clients, CloudName container, Func<string, bool> predicate, Maybe<int> count, ContinuationToken continuation)
		{
			CreateBlobContainer(clients.LiveBlobs, container.LiveName);
			Transfer(
				() => clients.SnapshotBlobs.GetContainerReference(container.SnapshotName),
				() => clients.LiveBlobs.GetContainerReference(container.LiveName),
				predicate, count, continuation);
		}

		public static IEnumerable<string> ListLiveContainerNames(this CloudClients clients)
		{
			return clients.LiveBlobs.ListContainers().Select(container => container.Name);
		}

		public static IEnumerable<CloudName> ListSnapshotBlobContainers(this CloudClients clients, string accountName, string snapshotId, IContainerNameMappingScheme scheme)
		{
			return clients.SnapshotBlobs.ListContainers(scheme.BuildSnapshotNamePrefix(accountName, snapshotId)).Select(container => scheme.LookupLiveName(accountName, snapshotId, container.Name));
		}

		public static void DeleteSnapshotBlobContainers(this CloudClients clients, string accountName, string snapshotId, IContainerNameMappingScheme scheme)
		{
			var client = clients.SnapshotBlobs;
			foreach (var snapshotName in clients.SnapshotBlobs.ListContainers(scheme.BuildSnapshotNamePrefix(accountName, snapshotId)).Select(container => container.Name).ToList())
			{
				client.GetContainerReference(snapshotName).Delete();
			}
		}

		static void CreateBlobContainer(CloudBlobClient client, string name)
		{
			var container = client.GetContainerReference(name);
			AzurePolicies.SlowInstantiation.Do(() => container.CreateIfNotExist());

			// verify
			var blob = container.GetBlobReference("__testblob");
			using (var stream = new MemoryStream())
			{
				AzurePolicies.SlowInstantiation.Do(() =>
				{
					blob.UploadFromStream(stream);
					blob.Delete();
				});
			}
		}

		static void Transfer(
			Func<CloudBlobContainer> sourceContainer,
			Func<CloudBlobContainer> targetContainer,
			Func<string, bool> blobPredicate,
			Maybe<int> count,
			ContinuationToken continuation)
		{
			Parallel.ForEach(
				ListBlobs(sourceContainer(), count, continuation).Where(blobPredicate),
				new ParallelOptions {MaxDegreeOfParallelism = 3},
				() => Tuple.From(sourceContainer(), targetContainer()),
				(name, state, containers) =>
					{
						CopyBlob(name, containers.Item1, containers.Item2);
						return containers;
					},
				containers => { });
		}

		static IEnumerable<string> ListBlobs(CloudBlobContainer container, Maybe<int> count, ContinuationToken continuation)
		{
			var options = new BlobRequestOptions
			{
				UseFlatBlobListing = true,
				BlobListingDetails = BlobListingDetails.None
			};

			var blobNameOffset = container.Name.Length + 2;
			var segment = AzurePolicies.TransientServerErrorBackOff.Get(() => container.ListBlobsSegmented(count.GetValue(512), continuation.Continuation.GetValue((ResultContinuation)null), options));

			while (true)
			{
				foreach (var blob in segment.Results)
				{
					yield return blob.Uri.AbsolutePath.Substring(blobNameOffset);
				}

				if (segment.HasMoreResults)
				{
					var segmentClosure = segment;
					segment = AzurePolicies.TransientServerErrorBackOff.Get(segmentClosure.GetNext);
					continue;
				}

				break;
			}

			continuation.Continuation = segment.ContinuationToken ?? Maybe<ResultContinuation>.Empty;
		}

		static void CopyBlob(string name, CloudBlobContainer sourceContainer, CloudBlobContainer targetContainer)
		{
			// note: we can't use the blob client's copy functionality, as it doesn't support copying between containers or even accounts

			var sourceBlob = sourceContainer.GetBlobReference(name);
			var targetBlob = targetContainer.GetBlobReference(name);

			// Download
			var stream = new MemoryStream();
			try
			{
				AzurePolicies.TransientServerErrorBackOff.Do(() => sourceBlob.DownloadToStream(stream));
			}
			catch (StorageException e)
			{
				if (e.ErrorCode == StorageErrorCode.BlobNotFound)
				{
					return;
				}

				throw;
			}

			// Copy Metadata
			var sourceMeta = sourceBlob.Metadata;
			var targetMeta = targetBlob.Metadata;
			foreach (var key in sourceMeta.AllKeys)
			{
				targetMeta[key] = sourceMeta[key];
			}

			// Upload
			stream.Seek(0, SeekOrigin.Begin);
			AzurePolicies.TransientServerErrorBackOff.Do(() => targetBlob.UploadFromStream(stream));
		}
	}
}
