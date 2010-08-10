#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System.Globalization;
using System.Linq;
using Lokad.Cloud.Storage;

namespace Lokad.Cloud.Snapshot.Cloud.Reports
{
	internal static class ReportEntities
	{
		public static CloudEntity<MonitoringIndicatorReport> ToCloudEntity(this MonitoringIndicatorReport monitoringIndicator)
		{
			var key = monitoringIndicator.Name.Replace("/", "");
			return new CloudEntity<MonitoringIndicatorReport>
			       	{
			       		PartitionKey = key,
			       		RowKey = key,
			       		Value = monitoringIndicator
			       	};
		}

		public static CloudEntity<MonitoringMessageReport> ToCloudEntity(this MonitoringMessageReport monitoringMessage)
		{
			return new CloudEntity<MonitoringMessageReport>
			       	{
			       		PartitionKey = monitoringMessage.Updated.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
			       		RowKey = monitoringMessage.Id,
			       		Value = monitoringMessage
			       	};
		}

		public static CloudEntity<CompleteSnapshotReport> ToCloudEntity(this CompleteSnapshotReport completeSnapshot)
		{
			return new CloudEntity<CompleteSnapshotReport>
			       	{
			       		PartitionKey = completeSnapshot.AccountName,
			       		RowKey = completeSnapshot.SnapshotId,
			       		Value = completeSnapshot
			       	};
		}
	}
}
