#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using Lokad.Cloud.Snapshot.Cloud.State;
using Lokad.Cloud.Snapshot.Framework;
using Lokad.Cloud.Storage;

namespace Lokad.Cloud.Snapshot.Cloud.Mapping
{
	public class UniqueNamingScheme : IContainerNameMappingScheme
	{
		private readonly CloudTable<ContainerState> _containers;

		public UniqueNamingScheme(CloudInfrastructureProviders providers)
		{
			_containers = new CloudTable<ContainerState>(providers.TableStorage, Names.ContainerStateTable);
		}

		public CloudName GenerateNewSnapshotName(string accountName, string snapshotId, string liveName)
		{
			return new CloudName
			       	{
			       		LiveName = liveName,
			       		SnapshotName = string.Concat("s", accountName, snapshotId, Guid.NewGuid().ToString("N"))
			       	};
		}

		public CloudName LookupLiveName(string accountName, string snapshotId, string snapshotName)
		{
			return new CloudName
			       	{
			       		SnapshotName = snapshotName,
			       		LiveName = _containers.GetContainerEntity(accountName, snapshotId, snapshotName).Value.Value.LiveName
			       	};
		}

		public string BuildSnapshotNamePrefix(string accountName, string snapshotId)
		{
			return string.Concat("s", accountName, snapshotId);
		}
	}
}
