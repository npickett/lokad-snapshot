#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using Lokad.Cloud.Snapshot.Cloud.Commands;
using Lokad.Cloud.Snapshot.Framework;

namespace Lokad.Cloud.Snapshot.Cloud.Handlers
{
	public abstract class TransferService<T> : HandlerService<T>
		where T : TransferCommand
	{
		protected abstract void DoTransfer(CloudName itemName, CloudClients clients, ContinuationToken continuationToken);
		protected abstract void OnCompleted(T message, string accountName, string snapshotId);

		protected override sealed void Handle(T message, CloudClients clients, string accountName, string snapshotId)
		{
			var doNotContinueAfter = DateTime.UtcNow.AddMinutes(30);

			do
			{
				DoTransfer(message.ItemName, clients, message.Continuation);
			} while (message.Continuation.HasContinuation && DateTime.UtcNow < doNotContinueAfter);

			if (message.Continuation.HasContinuation)
			{
				// reschedule the remaining parts with updated continuation token
				Put(message);
				return;
			}

			OnCompleted(message, message.AccountName, message.SnapshotId);
		}
	}
}
