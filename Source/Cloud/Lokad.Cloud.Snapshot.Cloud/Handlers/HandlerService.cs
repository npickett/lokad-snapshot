#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Snapshot.Cloud.Commands;
using Lokad.Cloud.Snapshot.Framework;

namespace Lokad.Cloud.Snapshot.Cloud.Handlers
{
	public abstract class HandlerService<T> : QueueService<T>
		where T : Command
	{
		protected abstract void Handle(T message, CloudClients clients, string accountName, string snapshotId);
		protected abstract void OnError(T message, Exception exception, string accountName, string snapshotId);

		protected sealed override void Start(T message)
		{
			try
			{
				var clients = new CloudClients(message.Credentials);
				Handle(message, clients, message.AccountName, message.SnapshotId);
			}
			catch (Exception ex)
			{
				OnError(message, ex, message.AccountName, message.SnapshotId);
				return;
			}
		}

	}
}
