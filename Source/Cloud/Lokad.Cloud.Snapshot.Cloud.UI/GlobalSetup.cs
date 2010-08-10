#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using Autofac;
using Autofac.Builder;

namespace Lokad.Cloud.Snapshot.Cloud.UI
{
	public static class GlobalSetup
	{
		public static readonly IContainer Container;

		static GlobalSetup()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new SnapshotCloudModule());
			Container = builder.Build();
		}
	}
}