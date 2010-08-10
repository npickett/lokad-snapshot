#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

namespace Lokad.Cloud.Snapshot.Cloud
{
	public static class Names
	{
		public const string IndicatorReportsTable = "LokadCloudSnapshotIndicators";
		public const string MessageReportsTable = "LokadCloudSnapshotMessages";
		public const string CompleteSnapshotReportsTable = "LokadCloudSnapshotCompleteSnapshots";

		public const string SnapshotStateTable = "LokadCloudSnapshots";
		public const string ContainerStateTable = "LokadCloudContainers";
		public const string ContainerRestoreStateTable = "LokadCloudContainerRestores";

		public const string ManagementConnectionStringConfig = "SnapshotManagementAccount";
		public const string LiveAccountsConfig = "LiveAccounts";
		public const string SnapshotIntervalDaysConfig = "SnapshotIntervalDays";
		public const string NumberOfSnapshotsToKeepConfig = "NumberOfSnapshotsToKeep";
	}
}
