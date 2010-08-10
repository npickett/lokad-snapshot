#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Linq;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Snapshot.Cloud.Reports;
using Lokad.Cloud.Storage;

namespace Lokad.Cloud.Snapshot.Cloud.Activators
{
	[ScheduledServiceSettings(AutoStart = true, TriggerInterval = 12 * 60 * 60)]
	public class AutoPruneOldReportsService : ScheduledService
	{
		protected override void StartOnSchedule()
		{
			var threshold = DateTime.UtcNow.AddMonths(-1);
			RemoveEntitiesOlderThan<MonitoringIndicatorReport>(Names.IndicatorReportsTable, threshold);
			RemoveEntitiesOlderThan<MonitoringMessageReport>(Names.MessageReportsTable, threshold);

			// no need to prune complete snapshot reports, they're automatically removed once the actual snapshot gets deleted.
			// RemoveEntitiesOlderThan<CompleteSnapshotReport>(Names.CompleteSnapshotReportsTable, threshold);
		}

		void RemoveEntitiesOlderThan<T>(string tableName, DateTime threshold)
		{
			var table = new CloudTable<T>(TableStorage, tableName);
			var toDelete = table.Get().Where(entity => entity.Timestamp < threshold).ToList();
			TableStorage.Delete(tableName, toDelete, false);
		}
	}
}
