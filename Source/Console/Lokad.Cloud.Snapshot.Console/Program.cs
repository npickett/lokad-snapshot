#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using Lokad.Cloud.Snapshot.Framework;
using Microsoft.WindowsAzure;

namespace ConsoleExample
{
	class Program
	{
		static void Main(string[] args)
		{
			var clients = new CloudClients(
				CloudStorageAccount.Parse("LIVE/ORIGINAL STORAGE ACCOUNT HERE"),
				CloudStorageAccount.Parse("SNAPSHOT STORAGE ACCOUNT HERE"));

			IContainerNameMappingScheme scheme = null; // provide used scheme here
			const string accountName = "foo";
			const string snapshotId = "bar";

			Console.WriteLine("Restore Blobs");
			foreach (var container in clients.ListSnapshotBlobContainers(accountName, snapshotId, scheme))
			{
				Console.WriteLine("  Container {0}", container.LiveName);
				var continuation = new ContinuationToken();
				do
				{
					clients.RestoreContainer(container, name => true, 128, continuation);
					Console.Write('.');
				} while (continuation.HasContinuation);
				Console.WriteLine();
			}

			Console.WriteLine("Restore Tables");
			foreach (var table in clients.ListSnapshotsTables(accountName, snapshotId, scheme))
			{
				Console.WriteLine("  Table {0}", table.LiveName);
				var continuation = new ContinuationToken();
				do
				{
					clients.RestoreTable(table, entity => true, 128, continuation);
					Console.Write('.');
				} while (continuation.HasContinuation);
				Console.WriteLine();
			}

			Console.WriteLine("Done");
		}
	}
}
