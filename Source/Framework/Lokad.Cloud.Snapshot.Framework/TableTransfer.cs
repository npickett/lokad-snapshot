#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Lokad.Cloud.Storage.Azure;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cloud.Snapshot.Framework
{
	public static class TableTransfer
	{
		public static void SnapshotTable(this CloudClients clients, CloudName table, Func<FatEntity, bool> predicate, Maybe<int> count, ContinuationToken continuation)
		{
			CreateTable(clients.SnapshotTables, table.SnapshotName);
			Transfer(
				() => clients.LiveTables.GetDataServiceContext(),
				() => clients.SnapshotTables.GetDataServiceContext(),
				table.LiveName, table.SnapshotName, predicate, count, continuation);
		}

		public static void RestoreTable(this CloudClients clients, CloudName table, Func<FatEntity, bool> predicate, Maybe<int> count, ContinuationToken continuation)
		{
			CreateTable(clients.LiveTables, table.LiveName);
			Transfer(
				() => clients.SnapshotTables.GetDataServiceContext(),
				() => clients.LiveTables.GetDataServiceContext(),
				table.SnapshotName, table.LiveName, predicate, count, continuation);
		}

		public static IEnumerable<string> ListLiveTableNames(this CloudClients clients)
		{
			return clients.LiveTables.ListTables();
		}

		public static IEnumerable<CloudName> ListSnapshotsTables(this CloudClients clients, string accountName, string snapshotId, IContainerNameMappingScheme scheme)
		{
			return clients.SnapshotTables.ListTables(scheme.BuildSnapshotNamePrefix(accountName, snapshotId)).Select(table => scheme.LookupLiveName(accountName, snapshotId, table));
		}

		public static void DeleteSnapshotTables(this CloudClients clients, string accountName, string snapshotId, IContainerNameMappingScheme scheme)
		{
			var client = clients.SnapshotTables;
			foreach (var snapshotName in clients.SnapshotTables.ListTables(scheme.BuildSnapshotNamePrefix(accountName, snapshotId)).ToList())
			{
				client.DeleteTableIfExist(snapshotName);
			}
		}

		static void CreateTable(CloudTableClient client, string name)
		{
			AzurePolicies.SlowInstantiation.Do(() => client.CreateTableIfNotExist(name));

			// verify
			var context = client.GetDataServiceContext();
			context.MergeOption = MergeOption.NoTracking;
			context.ResolveType = ResolveFatEntityType;
			var entity = new FatEntity { PartitionKey = "__testpartition", RowKey = "__testrow" };
			context.AddObject(name, entity);
			AzurePolicies.SlowInstantiation.Do(() => context.SaveChanges());
			context.DeleteObject(entity);
			AzurePolicies.SlowInstantiation.Do(() => context.SaveChanges());
		}

		static void Transfer(
			Func<TableServiceContext> sourceContext,
			Func<TableServiceContext> targetContext,
			string sourceTable,
			string targetTable,
			Func<FatEntity, bool> entityPredicate,
			Maybe<int> count,
			ContinuationToken continuation)
		{
			// NOTE: this is only really parallelized in case there are multiple partitions part of the current page, which is not necesarily the case.
			// TODO: Need to find a better way
			Parallel.ForEach(
				DownloadEntries(sourceContext(), sourceTable, Maybe.String, count, continuation).Where(entityPredicate).GroupBy(e => e.PartitionKey),
				new ParallelOptions {MaxDegreeOfParallelism = 3},
				() =>
					{
						var targetCtx = targetContext();
						targetCtx.MergeOption = MergeOption.NoTracking;
						targetCtx.ResolveType = ResolveFatEntityType;
						return targetCtx;
					},
				(partition, state, context) =>
					{
						var existingKeys = DownloadPartitionExistingRowKeys(context, targetTable, partition.Key);

						// Insert
						foreach (var slice in Slice(partition.Where(e => !existingKeys.Contains(e.RowKey))))
						{
							foreach (var entity in slice)
							{
								context.AddObject(targetTable, entity);
							}
							AzurePolicies.TransientTableErrorBackOff.Do(() => context.SaveChanges(SaveChangesOptions.Batch));
						}

						// Update
						foreach (var slice in Slice(partition.Where(e => existingKeys.Contains(e.RowKey))))
						{
							foreach (var entity in slice)
							{
								context.AttachTo(targetTable, entity, "*");
								context.UpdateObject(entity);
							}
							AzurePolicies.TransientTableErrorBackOff.Do(() => context.SaveChanges(SaveChangesOptions.Batch));
						}

						return context;
					},
				context => { });
		}

		static IEnumerable<FatEntity> DownloadEntries(
			DataServiceContext context,
			string tableName,
			Maybe<string> filter,
			Maybe<int> count,
			ContinuationToken continuation)
		{
			context.MergeOption = MergeOption.NoTracking;
			context.ResolveType = ResolveFatEntityType;

			var query = context.CreateQuery<FatEntity>(tableName);

			if (filter.HasValue)
			{
				query = query.AddQueryOption("$filter", filter.Value);
			}

			if (count.HasValue)
			{
				query = (DataServiceQuery<FatEntity>)query.Take(count.Value);
			}

			if (continuation.NextPartitionKey.HasValue)
			{
				query = query.AddQueryOption("NextPartitionKey", continuation.NextPartitionKey.Value);
			}

			if (continuation.NextRowKey.HasValue)
			{
				query = query.AddQueryOption("NextRowKey", continuation.NextRowKey.Value);
			}

			QueryOperationResponse response = null;
			FatEntity[] entities = null;

			AzurePolicies.TransientTableErrorBackOff.Do(() =>
			{
				try
				{
					response = query.Execute() as QueryOperationResponse;
					entities = ((IEnumerable<FatEntity>)response).ToArray();
				}
				catch (DataServiceQueryException ex)
				{
					// if the table does not exist, there is nothing to return
					var errorCode = AzurePolicies.GetErrorCode(ex);
					if (TableErrorCodeStrings.TableNotFound == errorCode)
					{
						entities = new FatEntity[0];
						return;
					}

					throw;
				}
			});

			continuation.NextPartitionKey = response != null && response.Headers.ContainsKey("x-ms-continuation-NextPartitionKey")
				? response.Headers["x-ms-continuation-NextPartitionKey"]
				: Maybe.String;

			continuation.NextRowKey = response != null && response.Headers.ContainsKey("x-ms-continuation-NextRowKey")
				? response.Headers["x-ms-continuation-NextRowKey"]
				: Maybe.String;

			return entities;
		}

		static HashSet<string> DownloadPartitionExistingRowKeys(
			DataServiceContext context,
			string tableName,
			string partitionKey)
		{
			var set = new HashSet<string>();
			var filter = string.Format("(PartitionKey eq '{0}')", HttpUtility.UrlEncode(partitionKey));
			var continuation = new ContinuationToken();
			do
			{
				set.UnionWith(DownloadEntries(context, tableName, filter, Maybe<int>.Empty, continuation).Select(e => e.RowKey));
			} while (continuation.HasContinuation);
			set.TrimExcess();
			return set;
		}

		static IEnumerable<FatEntity[]> Slice(IEnumerable<FatEntity> entities)
		{
			var accumulator = new List<FatEntity>(100);
			var payload = 0;
			foreach (var entity in entities)
			{
				var entityPayLoad = entity.GetPayload();
				if (accumulator.Count >= 100 || payload + entityPayLoad >= 0x3E0000)
				{
					yield return accumulator.ToArray();
					accumulator.Clear();
					payload = 0;
				}
				accumulator.Add(entity);
				payload += entityPayLoad;
			}

			if (accumulator.Count > 0)
			{
				yield return accumulator.ToArray();
			}
		}

		static Type ResolveFatEntityType(string name)
		{
			return typeof(FatEntity);
		}
	}
}
