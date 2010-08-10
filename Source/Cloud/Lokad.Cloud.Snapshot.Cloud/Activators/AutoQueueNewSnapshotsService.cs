#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Globalization;
using System.Linq;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Snapshot.Cloud.Commands;
using Lokad.Cloud.Snapshot.Cloud.State;
using Lokad.Cloud.Snapshot.Framework;
using Lokad.Cloud.Storage;

namespace Lokad.Cloud.Snapshot.Cloud.Activators
{
	[ScheduledServiceSettings(AutoStart = true, TriggerInterval = 3 * 60 * 60)]
	public class AutoQueueNewSnapshotsService : ScheduledService
	{
		protected override void StartOnSchedule()
		{
			var snapshots = new CloudTable<SnapshotState>(TableStorage, Names.SnapshotStateTable);
			var intervalDays = Double.Parse(CloudEnvironment.GetConfigurationSetting(Names.SnapshotIntervalDaysConfig).GetValue("7"), NumberFormatInfo.InvariantInfo);
			var notOlderThan = DateTime.UtcNow.AddDays(-intervalDays);

			var candidate = Accounts.ListAccounts()
				.Where(name => !snapshots.Get(name).Where(s => s.Value.Created > notOlderThan).Any())
				.FirstOrEmpty();

			if(!candidate.HasValue)
			{
				return;
			}

			var account = candidate.Value;
			Put(new StartSnapshotCommand
				{
					AccountName = account,
					SnapshotId = IdHelper.NewId(),
					Credentials = Accounts.GetCredentialsForAccount(account)
				});
		}
	}
}
