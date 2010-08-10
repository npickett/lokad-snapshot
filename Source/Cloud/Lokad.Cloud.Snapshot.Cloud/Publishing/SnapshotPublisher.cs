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
	public class SnapshotPublisher
	{
		private readonly CloudTable<MonitoringIndicatorReport> _indicators;
		private readonly CloudTable<MonitoringMessageReport> _messages;
		private readonly CloudTable<CompleteSnapshotReport> _completeSnapshots;

		private readonly CloudTable<SnapshotState> _snapshots;
		private readonly CloudTable<ContainerState> _containers;

		public SnapshotPublisher(CloudInfrastructureProviders providers)
		{
			// State:
			_snapshots = new CloudTable<SnapshotState>(providers.TableStorage, Names.SnapshotStateTable);
			_containers = new CloudTable<ContainerState>(providers.TableStorage, Names.ContainerStateTable);

			// Reports:
			_indicators = new CloudTable<MonitoringIndicatorReport>(providers.TableStorage, Names.IndicatorReportsTable);
			_messages = new CloudTable<MonitoringMessageReport>(providers.TableStorage, Names.MessageReportsTable);
			_completeSnapshots = new CloudTable<CompleteSnapshotReport>(providers.TableStorage, Names.CompleteSnapshotReportsTable);

		}

		public void SnapshotStarted(
			string accountName,
			string snapshotId,
			IEnumerable<CloudName> blobContainers,
			IEnumerable<CloudName> tables)
		{
			var now = DateTime.UtcNow;

			// State:
			_snapshots.Insert(BuildState.Snapshot(accountName, snapshotId, now));

			SnapshotTasksCreated(accountName, snapshotId, ContainerType.BlobContainer, now, blobContainers);
			SnapshotTasksCreated(accountName, snapshotId, ContainerType.Table, now, tables);

			// Reports:
			var blobContainersCount = blobContainers.Count();
			var tablesCount = tables.Count();
			_messages.Insert(BuildReport.Message(string.Format("Snapshot {0} for account {1} started", snapshotId, accountName), "snapshot status started", null));
			_indicators.Upsert(new[]
				{
					BuildReport.Indicator(string.Format("/snapshots/{0}/{1}/Status", accountName, snapshotId), "snapshot detail status started", "started"),
					BuildReport.Indicator(string.Format("/snapshots/{0}/{1}/Started", accountName, snapshotId), "snapshot detail date", now.ToString()),
					BuildReport.Indicator(string.Format("/snapshots/{0}/{1}/BlobContainers", accountName, snapshotId), "snapshot detail statistics", blobContainersCount.ToString()),
					BuildReport.Indicator(string.Format("/snapshots/{0}/{1}/Tables", accountName, snapshotId), "snapshot detail statistics", tablesCount.ToString()),
					BuildReport.Indicator(string.Format("/snapshots/{0}/Last/Id", accountName), "snapshot last snapshotid", snapshotId),
					BuildReport.Indicator(string.Format("/snapshots/{0}/Last/Status", accountName), "snapshot last status started", "started"),
					BuildReport.Indicator(string.Format("/snapshots/{0}/Last/Started", accountName), "snapshot last date", now.ToString()),
					BuildReport.Indicator(string.Format("/snapshots/{0}/Last/BlobContainers", accountName), "snapshot last statistics", blobContainersCount.ToString()),
					BuildReport.Indicator(string.Format("/snapshots/{0}/Last/Tables", accountName), "snapshot last statistics", tablesCount.ToString()),
				});
		}

		void SnapshotCompleted(string accountName, string snapshotId)
		{
			var now = DateTime.UtcNow;

			// State:
			var snapthotEntity = _snapshots.GetSnapshotEntity(accountName, snapshotId).Value;
			snapthotEntity.Value.IsCompleted = true;
			_snapshots.Update(snapthotEntity);

			// Reports:
			_completeSnapshots.Insert(BuildReport.CompleteSnapshot(accountName, snapshotId, now));
			_messages.Insert(BuildReport.Message(string.Format("Snapshot {0} for account {1} completed", snapshotId, accountName), "snapshot status success", null));
			_indicators.Upsert(new[]
				{
					BuildReport.Indicator(string.Format("/snapshots/{0}/{1}/Status", accountName, snapshotId), "snapshot detail status success", "completed"),
					BuildReport.Indicator(string.Format("/snapshots/{0}/{1}/Completed", accountName, snapshotId), "snapshot detail date", now.ToString()),
					BuildReport.Indicator(string.Format("/snapshots/{0}/Last/Status", accountName), "snapshot last status success", "completed"),
					BuildReport.Indicator(string.Format("/snapshots/{0}/LastSuccessful/Id", accountName), "snapshot lastsuccessful snapshotid", snapshotId),
					BuildReport.Indicator(string.Format("/snapshots/{0}/LastSuccessful/Completed", accountName), "snapshot lastsuccessful date", now.ToString())
				});
		}

		public void SnapshotFailed(string accountName, string snapshotId, Exception exception)
		{
			// State:
			var snapthotEntity = _snapshots.GetSnapshotEntity(accountName, snapshotId).Value;
			snapthotEntity.Value.IsFailed = true;
			_snapshots.Update(snapthotEntity);

			// Reports:
			_messages.Insert(BuildReport.Message(
				string.Format("Snapshot {0} for account {1} failed: {2}", snapshotId, accountName, exception.Message),
				string.Format("snapshot status fault {0}", exception.GetType().Name),
				exception.ToString()));

			_indicators.Upsert(new[]
				{
					BuildReport.Indicator(string.Format("/snapshots/{0}/{1}/Status", accountName, snapshotId), "snapshot detail status fault", "failed"),
					BuildReport.Indicator(string.Format("/snapshots/{0}/Last/Status", accountName), "snapshot last status fault", "failed")
				});
		}

		public void SnapshotDeleted(string accountName, string snapshotId)
		{
			// State:
			_snapshots.Delete("snapshots", snapshotId);
			_containers.DeleteAllContainers(accountName, snapshotId);

			// Reports:
			_completeSnapshots.Delete(accountName, snapshotId);
			_messages.Insert(BuildReport.Message(string.Format("Snapshot {0} for account {1} deleted", snapshotId, accountName), "snapshot status deleted", null));
			_indicators.Upsert(BuildReport.Indicator(string.Format("/snapshots/{0}/{1}/Status", accountName, snapshotId), "snapshot detail status deleted", "deleted"));
		}

		public void DeleteSnapshotFailed(string accountName, string snapshotId, Exception exception)
		{
			// Reports:
			_messages.Insert(BuildReport.Message(
				string.Format("Snapshot {0} for account {1} deletion failed: {2}", snapshotId, accountName, exception.Message),
				string.Format("snapshot deletion fault {0}", exception.GetType().Name),
				exception.ToString()));
		}

		void SnapshotTasksCreated(string accountName, string snapshotId, ContainerType type, DateTime created, IEnumerable<CloudName> names)
		{
			// State:
			_containers.Insert(names.Select(name => BuildState.Container(accountName, snapshotId, created, type, PackingType.DirectCopy, name.LiveName, name.SnapshotName)));
		}

		public void SnapshotTaskCompleted(string accountName, string snapshotId, ContainerType type, CloudName name)
		{
			// State:
			var containerEntity = _containers.GetContainerEntity(accountName, snapshotId, name.SnapshotName).Value;
			containerEntity.Value.IsCompleted = true;
			_containers.Update(containerEntity);

			if(_containers.ListContainerEntities(accountName, snapshotId).All(container => container.Value.IsCompleted))
			{
				SnapshotCompleted(accountName, snapshotId);
			}
		}

		public void SnapshotTaskFailed(string accountName, string snapshotId, ContainerType type, CloudName name, Exception exception)
		{
			// State:
			var containerEntity = _containers.GetContainerEntity(accountName, snapshotId, name.SnapshotName).Value;
			containerEntity.Value.IsFailed = true;
			_containers.Update(containerEntity);

			// Reports:
			_messages.Insert(BuildReport.Message(
				string.Format("Snapshot {0} for account {1} of {2} {3} failed: {4}", snapshotId, accountName, type, name.LiveName, exception.Message),
				string.Format("fault snapshot {0}", exception.GetType().Name),
				exception.ToString()));

			_indicators.Upsert(new[]
				{
					BuildReport.Indicator(string.Format("/snapshots/{0}/{1}/Status", accountName, snapshotId), "snapshot detail status fault", "failed"),
					BuildReport.Indicator(string.Format("/snapshots/{0}/Last/Status", accountName), "snapshot last status fault", "failed")
				});
		}
	}
}
