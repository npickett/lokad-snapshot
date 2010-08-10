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
	public class DeleteSnapshotService : HandlerService<DeleteSnapshotCommand>
	{
		public SnapshotPublisher Publisher { get; set; }
		public IContainerNameMappingScheme NamingScheme { get; set; }

		protected override void Handle(DeleteSnapshotCommand message, CloudClients clients, string accountName, string snapshotId)
		{
			Publisher.SnapshotDeleted(accountName, snapshotId);

			clients.DeleteSnapshotBlobContainers(accountName, snapshotId, NamingScheme);
			clients.DeleteSnapshotTables(accountName, snapshotId, NamingScheme);
		}

		protected override void OnError(DeleteSnapshotCommand message, Exception exception, string accountName, string snapshotId)
		{
			Publisher.DeleteSnapshotFailed(accountName, snapshotId, exception);
		}
	}
}
