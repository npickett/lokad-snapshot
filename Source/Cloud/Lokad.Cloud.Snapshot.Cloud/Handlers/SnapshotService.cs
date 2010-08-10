#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Linq;
using Lokad.Cloud.Snapshot.Cloud.Commands;
using Lokad.Cloud.Snapshot.Cloud.Publishing;
using Lokad.Cloud.Snapshot.Framework;

namespace Lokad.Cloud.Snapshot.Cloud.Handlers
{
	public class SnapshotService : HandlerService<StartSnapshotCommand>
	{
		public SnapshotPublisher Publisher { get; set; }
		public IContainerNameMappingScheme NamingScheme { get; set; }

		protected override void Handle(StartSnapshotCommand message, CloudClients clients, string accountName, string snapshotId)
		{
			var containers = clients.ListLiveContainerNames().Select(name => NamingScheme.GenerateNewSnapshotName(accountName, snapshotId, name)).ToList();
			var tables = clients.ListLiveTableNames().Select(name => NamingScheme.GenerateNewSnapshotName(accountName, snapshotId, name)).ToList();

			Publisher.SnapshotStarted(accountName, snapshotId, containers, tables);

			PutRange(containers.Select(container => new StartSnapshotBlobCommand
				{
					AccountName = accountName,
					SnapshotId = snapshotId,
					Credentials = message.Credentials,
					ItemName = container,
					Continuation = new ContinuationToken()
				}));

			PutRange(tables.Select(table => new StartSnapshotTableCommand
				{
					AccountName = accountName,
					SnapshotId = snapshotId,
					Credentials = message.Credentials,
					ItemName = table,
					Continuation = new ContinuationToken()
				}));
		}

		protected override void OnError(StartSnapshotCommand message, Exception exception, string accountName, string snapshotId)
		{
			Publisher.SnapshotFailed(accountName, snapshotId, exception);
		}
	}
}
