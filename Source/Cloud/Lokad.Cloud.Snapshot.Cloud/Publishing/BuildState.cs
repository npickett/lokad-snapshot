#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using Lokad.Cloud.Snapshot.Cloud.State;
using Lokad.Cloud.Storage;

namespace Lokad.Cloud.Snapshot.Cloud.Publishing
{
	internal static class BuildState
	{
		public static CloudEntity<SnapshotState> Snapshot(string accountName, string snapshotId, DateTime created)
		{
			return new SnapshotState
			       	{
						AccountName = accountName,
						SnapshotId = snapshotId,
						Created = created
			       	}.ToCloudEntity();
		}

		public static CloudEntity<ContainerState> Container(string accountName, string snapshotId, DateTime created, ContainerType containerType, PackingType packingType, string liveName, string snapshotName)
		{
			return new ContainerState
			       	{
						AccountName = accountName,
						SnapshotId = snapshotId,
						Created = created,
						ContainerType = containerType,
						PackingType = packingType,
						LiveName = liveName,
						SnapshotName = snapshotName
			       	}.ToCloudEntity();
		}

		public static CloudEntity<ContainerRestoreState> ContainerRestore(string snapshotId, string restoreId, DateTime created, ContainerType containerType, string liveName)
		{
			return new ContainerRestoreState
			       	{
			       		SnapshotId = snapshotId,
			       		RestoreId = restoreId,
			       		Name = liveName,
			       		Type = containerType,
			       		Created = created
			       	}.ToCloudEntity();
		}
	}
}
