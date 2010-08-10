#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Runtime.Serialization;

namespace Lokad.Cloud.Snapshot.Cloud.Reports
{
	[Serializable]
	[DataContract(Name = "snapshot", Namespace = "http://schemas.lokad.com/snapshot/1.0/")]
	public class CompleteSnapshotReport
	{
		[DataMember(IsRequired = true)]
		public string AccountName { get; set; }

		[DataMember(IsRequired = true)]
		public string SnapshotId { get; set; }

		[DataMember(IsRequired = false)]
		public DateTime Completed { get; set; }
	}
}
