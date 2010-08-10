#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

namespace Lokad.Cloud.Snapshot.Framework
{
	public interface IContainerNameMappingScheme
	{
		CloudName GenerateNewSnapshotName(string accountName, string snapshotId, string liveName);
		CloudName LookupLiveName(string accountName, string snapshotId, string snapshotName);
		string BuildSnapshotNamePrefix(string accountName, string snapshotId);
	}
}
