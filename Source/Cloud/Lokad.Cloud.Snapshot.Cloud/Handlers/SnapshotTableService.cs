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
	public class SnapshotTableService : TransferService<StartSnapshotTableCommand>
	{
		public SnapshotPublisher Publisher { get; set; }

		protected override void DoTransfer(CloudName itemName, CloudClients clients, ContinuationToken continuationToken)
		{
			clients.SnapshotTable(itemName, entity => true, 512, continuationToken);
		}

		protected override void OnCompleted(StartSnapshotTableCommand message, string accountName, string snapshotId)
		{
			Publisher.SnapshotTaskCompleted(accountName, snapshotId, ContainerType.Table, message.ItemName);
		}

		protected override void OnError(StartSnapshotTableCommand message, Exception exception, string accountName, string snapshotId)
		{
			Publisher.SnapshotTaskFailed(accountName, snapshotId, ContainerType.Table, message.ItemName, exception);
		}
	}
}
