#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Linq;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Snapshot.Cloud.Commands;
using Lokad.Cloud.Snapshot.Cloud.Reports;
using Lokad.Cloud.Storage;

namespace Lokad.Cloud.Snapshot.Cloud.Activators
{
	[ScheduledServiceSettings(AutoStart = true, TriggerInterval = 3 * 60 * 60)] 
	public class AutoQueueOldSnapshotsForRemovalService : ScheduledService
	{
		protected override void StartOnSchedule()
		{
			var snapshotsToKeep = int.Parse(CloudEnvironment.GetConfigurationSetting(Names.NumberOfSnapshotsToKeepConfig).GetValue("3"));
			var accounts = Accounts.ListAccounts().ToSet();

			var snapshotToDelete = new CloudTable<CompleteSnapshotReport>(TableStorage, Names.CompleteSnapshotReportsTable).Get()
				.GroupBy(entity => entity.PartitionKey)
				.Where(group => accounts.Contains(group.Key, StringComparer.OrdinalIgnoreCase) && group.Count() > snapshotsToKeep)
				.Select(group => group.OrderBy(entity => entity.Value.Completed).First())
				.OrderBy(entity => entity.Value.Completed)
				.FirstOrEmpty();

			if(!snapshotToDelete.HasValue)
			{
				return;
			}

			var snapshot = snapshotToDelete.Value.Value;
			Put(new DeleteSnapshotCommand
				{
					AccountName = snapshot.AccountName,
					SnapshotId = snapshot.SnapshotId,
					Credentials = Accounts.BuildSnapshotOnlyCredentials()
				});
		}
	}
}
