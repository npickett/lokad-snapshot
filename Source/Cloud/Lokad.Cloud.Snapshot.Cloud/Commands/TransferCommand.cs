#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using Lokad.Cloud.Snapshot.Framework;

namespace Lokad.Cloud.Snapshot.Cloud.Commands
{
	[Serializable]
	public class TransferCommand : Command
	{
		public CloudName ItemName { get; set; }
		public ContinuationToken Continuation { get; set; }
	}
}
