#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Runtime.Serialization;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cloud.Snapshot.Framework
{
	[DataContract]
	[Serializable]
	public class ContinuationToken
	{
		[DataMember]
		public Maybe<ResultContinuation> Continuation { get; set; }

		[DataMember]
		public Maybe<string> NextPartitionKey { get; set; }

		[DataMember]
		public Maybe<string> NextRowKey { get; set; }

		public ContinuationToken()
		{
			Continuation = Maybe<ResultContinuation>.Empty;
			NextPartitionKey = Maybe.String;
			NextRowKey = Maybe.String;
		}

		public bool HasContinuation
		{
			get { return Continuation.HasValue || NextPartitionKey.HasValue || NextRowKey.HasValue; }
		}
	}
}
