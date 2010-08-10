#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Lokad.Cloud.Snapshot.Cloud.Reports;
using Lokad.Cloud.Snapshot.Cloud.State;
using Lokad.Cloud.Snapshot.Framework;
using Lokad.Cloud.Storage;

namespace Lokad.Cloud.Snapshot.Cloud.Publishing
{
	public class RestorePublisher
	{
		private readonly CloudTable<MonitoringIndicatorReport> _indicators;
		private readonly CloudTable<MonitoringMessageReport> _messages;

		private readonly CloudTable<ContainerRestoreState> _containerRestores;

		public RestorePublisher(CloudInfrastructureProviders providers)
		{
			// State:
			_containerRestores = new CloudTable<ContainerRestoreState>(providers.TableStorage, Names.ContainerRestoreStateTable);

			// Reports:
			_indicators = new CloudTable<MonitoringIndicatorReport>(providers.TableStorage, Names.IndicatorReportsTable);
			_messages = new CloudTable<MonitoringMessageReport>(providers.TableStorage, Names.MessageReportsTable);
		}

		public void RestoreStarted(
			string accountName,
			string snapshotId,
			string restoreId,
			string targetLiveAccountName,
			IEnumerable<CloudName> blobContainers,
			IEnumerable<CloudName> tables)
		{
			var now = DateTime.UtcNow;

			// State:
			RestoreTasksCreated(snapshotId, restoreId, ContainerType.BlobContainer, now, blobContainers);
			RestoreTasksCreated(snapshotId, restoreId, ContainerType.Table, now, tables);

			// Reports:
			var blobContainersCount = blobContainers.Count();
			var tablesCount = tables.Count();
			_messages.Insert(BuildReport.Message(string.Format("Restore {0} for snapshot {1} of account {2} to account {3} started", restoreId, snapshotId, accountName, targetLiveAccountName), "restore status started", null));
			_indicators.Upsert(new[]
				{
					BuildReport.Indicator(string.Format("/restores/{0}/{1}/Status", accountName, restoreId), "restore status started", "started"),
					BuildReport.Indicator(string.Format("/restores/{0}/{1}/Snapshot", accountName, restoreId), "restore snapshotid", snapshotId),
					BuildReport.Indicator(string.Format("/restores/{0}/{1}/Started", accountName, restoreId), "restore date", now.ToString()),
					BuildReport.Indicator(string.Format("/restores/{0}/{1}/BlobContainers", accountName, restoreId), "restore statistics", blobContainersCount.ToString()),
					BuildReport.Indicator(string.Format("/restores/{0}/{1}/Tables", accountName, restoreId), "restore statistics", tablesCount.ToString())
				});
		}

		private void RestoreCompleted(string accountName, string snapshotId, string restoreId)
		{
			// Reports:
			var now = DateTime.UtcNow;
			_messages.Insert(BuildReport.Message(string.Format("Restore {0} for snapshot {1} of account {2} completed", restoreId, snapshotId, accountName), "restore status success", null));
			_indicators.Upsert(new[]
				{
					BuildReport.Indicator(string.Format("/restores/{0}/{1}/Status", accountName, restoreId), "restore status success", "completed"),
					BuildReport.Indicator(string.Format("/restores/{0}/{1}/Completed", accountName, restoreId), "restore date", now.ToString()),
					BuildReport.Indicator(string.Format("/restores/{0}/LastSuccessful/Snapshot", accountName), "restore lastsuccessful snapshotid", snapshotId),
					BuildReport.Indicator(string.Format("/restores/{0}/LastSuccessful/Restore", accountName), "restore lastsuccessful restoreid", restoreId),
					BuildReport.Indicator(string.Format("/restores/{0}/LastSuccessful/Completed", accountName), "restore lastsuccessful date", now.ToString())
				});
		}

		public void RestoreFailed(string accountName, string snapshotId, string restoreId, Exception exception)
		{
			// Reports:
			_messages.Insert(BuildReport.Message(
				string.Format("Restore {0} for snapshot {1} of account {2} failed: {3}", restoreId, snapshotId, accountName, exception.Message),
				string.Format("restore status fault {0} ", exception.GetType().Name),
				exception.ToString()));

			_indicators.Upsert(BuildReport.Indicator(string.Format("/restores/{0}/{1}/Status", accountName, restoreId), "restore status fault", "failed"));
		}

		void RestoreTasksCreated(string snapshotId, string restoreId, ContainerType type, DateTime created, IEnumerable<CloudName> names)
		{
			// State:
			_containerRestores.Insert(names.Select(name => BuildState.ContainerRestore(snapshotId, restoreId, created, type, name.LiveName)));
		}

		public void RestoreTaskCompleted(string accountName, string snapshotId, string restoreId, ContainerType type, CloudName name)
		{
			// State:
			var entity = _containerRestores.GetContainerRestoreEntity(restoreId, type, name.LiveName).Value;
			entity.Value.IsCompleted = true;
			_containerRestores.Update(entity);

			// Denormalization:
			if (!_containerRestores.Get(restoreId).Any(task => !task.Value.IsCompleted))
			{
				RestoreCompleted(accountName, snapshotId, restoreId);
			}
		}

		public void RestoreTaskFailed(string accountName, string snapshotId, string restoreId, ContainerType type, CloudName name, Exception exception)
		{
			// State:
			var entity = _containerRestores.GetContainerRestoreEntity(restoreId, type, name.LiveName).Value;
			entity.Value.IsFailed = true;
			_containerRestores.Update(entity);

			// Reports:
			_messages.Insert(BuildReport.Message(
				string.Format("Restore {0} for snapshot {1} of account {2} of {3} {4} failed: {5}", restoreId, snapshotId, accountName, type, name.LiveName, exception.Message),
				string.Format("restore status fault {0} ", exception.GetType().Name),
				exception.ToString()));

			_indicators.Upsert(BuildReport.Indicator(string.Format("/restores/{0}/{1}/Status", accountName, restoreId), "restore status fault", "failed"));
		}
	}
}
