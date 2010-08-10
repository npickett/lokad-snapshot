#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using Lokad.Cloud.Snapshot.Cloud.Commands;
using Lokad.Cloud.Snapshot.Cloud.Publishing;
using Lokad.Cloud.Snapshot.Framework;

namespace Lokad.Cloud.Snapshot.Cloud.Handlers
{
	public class RestoreBlobService : TransferService<StartRestoreBlobCommand>
	{
		public RestorePublisher Publisher { get; set; }

		protected override void DoTransfer(CloudName itemName, CloudClients clients, ContinuationToken continuationToken)
		{
			clients.RestoreContainer(itemName, name => true, 512, continuationToken);
		}

		protected override void OnCompleted(StartRestoreBlobCommand message, string accountName, string snapshotId)
		{
			Publisher.RestoreTaskCompleted(accountName, snapshotId, message.RestoreId, ContainerType.BlobContainer, message.ItemName);
		}

		protected override void OnError(StartRestoreBlobCommand message, Exception exception, string accountName, string snapshotId)
		{
			Publisher.RestoreTaskFailed(accountName, snapshotId, message.RestoreId, ContainerType.BlobContainer, message.ItemName, exception);
		}
	}
}
