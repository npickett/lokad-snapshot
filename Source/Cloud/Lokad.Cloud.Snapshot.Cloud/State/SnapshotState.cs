#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Runtime.Serialization;

namespace Lokad.Cloud.Snapshot.Cloud.State
{
	[DataContract(Namespace = "http://schemas.lokad.com/snapshot/1.0/")]
	public class SnapshotState
	{
		[DataMember(IsRequired = true)]
		public string AccountName { get; set; }

		[DataMember(IsRequired = true)]
		public string SnapshotId { get; set; }

		[DataMember(IsRequired = true)]
		public DateTime Created { get; set; }

		[DataMember(IsRequired = false)]
		public bool IsCompleted { get; set; }

		[DataMember(IsRequired = false)]
		public bool IsFailed { get; set; }
	}
}
