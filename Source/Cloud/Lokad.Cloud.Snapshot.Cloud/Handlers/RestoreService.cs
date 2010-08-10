#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System.Linq;
using Lokad.Cloud.Snapshot.Cloud.Commands;
using Lokad.Cloud.Snapshot.Cloud.Publishing;
using Lokad.Cloud.Snapshot.Framework;

namespace Lokad.Cloud.Snapshot.Cloud.Handlers
{
	public class RestoreService : HandlerService<StartRestoreCommand>
	{
		public RestorePublisher Publisher { get; set; }
		public IContainerNameMappingScheme NamingScheme { get; set; }

		protected override void Handle(StartRestoreCommand message, CloudClients clients, string accountName, string snapshotId)
		{
			var containers = clients.ListSnapshotBlobContainers(accountName, snapshotId, NamingScheme).ToList();
			var tables = clients.ListSnapshotsTables(accountName, snapshotId, NamingScheme).ToList();

			Publisher.RestoreStarted(accountName, snapshotId, message.RestoreId, clients.LiveAccountName, containers, tables);

			PutRange(containers.Select(container => new StartRestoreBlobCommand
				{
					AccountName = accountName,
					RestoreId = message.RestoreId,
					SnapshotId = snapshotId,
					Credentials = message.Credentials,
					ItemName = container,
					Continuation = new ContinuationToken()
				}));

			PutRange(tables.Select(table => new StartRestoreTableCommand
				{
					AccountName = accountName,
					RestoreId = message.RestoreId,
					SnapshotId = snapshotId,
					Credentials = message.Credentials,
					ItemName = table,
					Continuation = new ContinuationToken()
				}));
		}

		protected override void OnError(StartRestoreCommand message, System.Exception exception, string accountName, string snapshotId)
		{
			Publisher.RestoreFailed(accountName, snapshotId, message.RestoreId, exception);
		}
	}
}
