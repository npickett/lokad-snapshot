#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using Lokad.Cloud.Snapshot.Cloud.Reports;
using Lokad.Cloud.Storage;

namespace Lokad.Cloud.Snapshot.Cloud.Publishing
{
	internal static class BuildReport
	{
		public static CloudEntity<MonitoringIndicatorReport> Indicator(string name, string tags, string value)
		{
			return new MonitoringIndicatorReport
			       	{
			       		Name = name,
			       		Updated = DateTime.UtcNow,
			       		Tags = tags,
			       		Value = value
			       	}.ToCloudEntity();
		}

		public static CloudEntity<MonitoringMessageReport> Message(string title, string tags, string summary)
		{
			return new MonitoringMessageReport
			       	{
			       		Id = Guid.NewGuid().ToString(),
			       		Updated = DateTime.UtcNow,
			       		Title = title,
			       		Tags = tags,
			       		Summary = summary
			       	}.ToCloudEntity();
		}

		public static CloudEntity<CompleteSnapshotReport> CompleteSnapshot(string accountName, string snapshotId, DateTime completed)
		{
			return new CompleteSnapshotReport
			       	{
			       		AccountName = accountName,
			       		SnapshotId = snapshotId,
			       		Completed = completed
			       	}.ToCloudEntity();
		}
	}
}
