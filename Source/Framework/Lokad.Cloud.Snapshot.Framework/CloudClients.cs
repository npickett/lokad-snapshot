#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cloud.Snapshot.Framework
{
	/// <summary>
	/// Thread-Local Cloud Clients for easy parallelization
	/// </summary>
	public class CloudClients
	{
		private readonly CloudStorageAccount _liveAccount;
		private readonly CloudStorageAccount _snapshotAccount;

		private readonly LocalDataStoreSlot _liveBlobsStore;
		private readonly LocalDataStoreSlot _liveTablesStore;
		private readonly LocalDataStoreSlot _snapshotBlobsStore;
		private readonly LocalDataStoreSlot _snapshotTablesStore;

		public CloudClients(CloudStorageAccount liveAccount, CloudStorageAccount snapshotAccount)
		{
			_liveAccount = liveAccount;
			_snapshotAccount = snapshotAccount;

			_liveBlobsStore = Thread.AllocateDataSlot();
			_liveTablesStore = Thread.AllocateDataSlot();
			_snapshotBlobsStore = Thread.AllocateDataSlot();
			_snapshotTablesStore = Thread.AllocateDataSlot();
		}

		public CloudClients(CloudCredentials credentials)
		{
			if (!string.IsNullOrEmpty(credentials.LiveConnectionString))
			{
				_liveAccount = CloudStorageAccount.Parse(credentials.LiveConnectionString);
			}

			_snapshotAccount = CloudStorageAccount.Parse(credentials.SnapshotConnectionString);

			_liveBlobsStore = Thread.AllocateDataSlot();
			_liveTablesStore = Thread.AllocateDataSlot();
			_snapshotBlobsStore = Thread.AllocateDataSlot();
			_snapshotTablesStore = Thread.AllocateDataSlot();
		}

		public string LiveAccountName
		{
			get { return _liveAccount.Credentials.AccountName; }
		}

		public string SnapshotAccountName
		{
			get { return _snapshotAccount.Credentials.AccountName; }
		}

		public CloudBlobClient LiveBlobs
		{
			get { return GetThreadLocal(_liveBlobsStore, () => Configure(_liveAccount.CreateCloudBlobClient())); }
		}

		public CloudTableClient LiveTables
		{
			get { return GetThreadLocal(_liveTablesStore, () => Configure(_liveAccount.CreateCloudTableClient())); }
		}

		public CloudBlobClient SnapshotBlobs
		{
			get { return GetThreadLocal(_snapshotBlobsStore, () => Configure(_snapshotAccount.CreateCloudBlobClient())); }
		}

		public CloudTableClient SnapshotTables
		{
			get { return GetThreadLocal(_snapshotTablesStore, () => Configure(_snapshotAccount.CreateCloudTableClient())); }
		}

		static T GetThreadLocal<T>(LocalDataStoreSlot slot, Func<T> factory)
			where T : class
		{
			var value = (T)Thread.GetData(slot);
			if (value == null)
			{
				value = factory();
				Thread.SetData(slot, value);
			}

			return value;
		}

		static CloudBlobClient Configure(CloudBlobClient client)
		{
			client.Timeout = 5.Minutes();
			client.RetryPolicy = RetryPolicies.RetryExponential(10, 1.Seconds());
			return client;
		}

		static CloudTableClient Configure(CloudTableClient client)
		{
			client.Timeout = 5.Minutes();
			client.RetryPolicy = RetryPolicies.RetryExponential(10, 1.Seconds());
			return client;
		}
	}
}
