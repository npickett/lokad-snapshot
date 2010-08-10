#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Globalization;

namespace Lokad.Cloud.Snapshot.Framework
{
	public static class IdHelper
	{
		public static string NewId()
		{
			return DateTime.UtcNow.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
		}
	}
}
