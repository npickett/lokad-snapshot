#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System.Runtime.Serialization;

namespace Lokad.Cloud.Snapshot.Framework
{
	[DataContract]
	public class CloudName
	{
		[DataMember(IsRequired = true)]
		public string LiveName { get; set; }

		[DataMember(IsRequired = true)]
		public string SnapshotName { get; set; }
	}
}
