#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System.Runtime.Serialization;

namespace Lokad.Cloud.Snapshot.Cloud
{
	[DataContract(Namespace = "http://schemas.lokad.com/snapshot/1.0/")]
	public enum PackingType
	{
		[EnumMember]
		DirectCopy
	}
}
