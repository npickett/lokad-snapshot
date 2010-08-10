#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Net;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.ServiceFabric.Runtime;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Lokad.Cloud.Snapshot.Cloud.Worker
{
	public class WorkerRole : RoleEntryPoint
	{
		private Scheduler _scheduler;

		public override bool OnStart()
		{
			ServicePointManager.DefaultConnectionLimit = 48;
			ServicePointManager.Expect100Continue = false;
			ServicePointManager.UseNagleAlgorithm = false;

			RoleEnvironment.Changing += RoleEnvironmentChanging;

			_scheduler = new Scheduler(
				() => GlobalSetup.Container.Resolve<IEnumerable<CloudService>>(),
				service => service.Start());

			return base.OnStart();
		}

		public override void OnStop()
		{
			if (_scheduler != null)
			{
				_scheduler.AbortWaitingSchedule();
				_scheduler = null;
			}

			base.OnStop();
		}

		public override void Run()
		{
			foreach(var action in _scheduler.Schedule())
			{
				action();
			}
		}

		private static void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
		{
			// If a configuration setting is changing
			if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
			{
				// Set e.Cancel to true to restart this role instance
				e.Cancel = true;
			}
		}
	}
}
