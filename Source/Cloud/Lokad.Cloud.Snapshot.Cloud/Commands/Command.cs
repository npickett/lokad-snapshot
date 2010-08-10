#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using Lokad.Cloud.Snapshot.Framework;

namespace Lokad.Cloud.Snapshot.Cloud.Commands
{
	[Serializable]
	public abstract class Command
	{
		public string AccountName { get; set; }
		public string SnapshotId { get; set; }
		public CloudCredentials Credentials { get; set; }
	}
}
