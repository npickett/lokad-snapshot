#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using Lokad.Cloud.Snapshot.Framework;

namespace Lokad.Cloud.Snapshot.Cloud.Mapping
{
	public class PrefixNamingScheme : IContainerNameMappingScheme
	{
		private const string SnapshotPrefix = "lcs";

		public CloudName GenerateNewSnapshotName(string accountName, string snapshotId, string liveName)
		{
			return new CloudName
			       	{
			       		LiveName = liveName,
			       		SnapshotName = SnapshotPrefix + snapshotId + liveName
			       	};
		}

		public CloudName LookupLiveName(string accountName, string snapshotId, string snapshotName)
		{
			if (!snapshotName.StartsWith(SnapshotPrefix + snapshotId))
			{
				throw new ArgumentException("Invalid Snapshot Name");
			}

			return new CloudName
			       	{
			       		SnapshotName = snapshotName,
			       		LiveName = snapshotName.Substring(SnapshotPrefix.Length + snapshotId.Length)
			       	};
		}

		public string BuildSnapshotNamePrefix(string accountName, string snapshotId)
		{
			return SnapshotPrefix + snapshotId;
		}
	}
}
