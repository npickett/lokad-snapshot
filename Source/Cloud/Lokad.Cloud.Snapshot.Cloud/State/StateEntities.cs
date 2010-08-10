#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System.Collections.Generic;
using System.Linq;
using Lokad.Cloud.Storage;

namespace Lokad.Cloud.Snapshot.Cloud.State
{
	internal static class StateEntities
	{
		public static CloudEntity<SnapshotState> ToCloudEntity(this SnapshotState snapshot)
		{
			return new CloudEntity<SnapshotState>
					{
						PartitionKey = snapshot.AccountName,
						RowKey = snapshot.SnapshotId,
						Value = snapshot
					};
		}

		public static CloudEntity<ContainerState> ToCloudEntity(this ContainerState container)
		{
			return new CloudEntity<ContainerState>
					{
						PartitionKey = container.AccountName + container.SnapshotId,
						RowKey = container.SnapshotName,
						Value = container
					};
		}

		public static CloudEntity<ContainerRestoreState> ToCloudEntity(this ContainerRestoreState containerRestore)
		{
			return new CloudEntity<ContainerRestoreState>
			{
				PartitionKey = containerRestore.RestoreId,
				RowKey = string.Concat(containerRestore.Type, "-", containerRestore.Name),
				Value = containerRestore
			};
		}

		public static Maybe<CloudEntity<SnapshotState>> GetSnapshotEntity(this CloudTable<SnapshotState> table, string accountName, string snapshotId)
		{
			return table.Get(accountName, snapshotId);
		}

		public static Maybe<CloudEntity<ContainerState>> GetContainerEntity(this CloudTable<ContainerState> table, string accountName, string snapshotId, string snapshotName)
		{
			return table.Get(accountName + snapshotId, snapshotName);
		}

		public static Maybe<CloudEntity<ContainerRestoreState>> GetContainerRestoreEntity(this CloudTable<ContainerRestoreState> table, string restoreId, ContainerType type, string liveName)
		{
			return table.Get(restoreId, string.Concat(type, "-", liveName));
		}

		public static IEnumerable<CloudEntity<ContainerState>> ListContainerEntities(this CloudTable<ContainerState> table, string accountName, string snapshotId)
		{
			return table.Get(accountName + snapshotId);
		}

		public static void DeleteAllContainers(this CloudTable<ContainerState> table, string accountName, string snapshotId)
		{
			var rowKeys = table.Get(accountName + snapshotId).Select(entity => entity.RowKey).ToList();
			table.Delete(accountName + snapshotId, rowKeys);
		}
	}
}
