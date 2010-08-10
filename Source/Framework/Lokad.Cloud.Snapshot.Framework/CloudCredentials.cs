#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System.Runtime.Serialization;

namespace Lokad.Cloud.Snapshot.Framework
{
	[DataContract]
	public class CloudCredentials
	{
		[DataMember(IsRequired = false)]
		public string LiveConnectionString { get; set; }

		[DataMember(IsRequired = false)]
		public string SnapshotConnectionString { get; set; }
	}
}
