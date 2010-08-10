#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System.Linq;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Lokad.Cloud.Snapshot.Cloud.UI
{
	public class WebRole : RoleEntryPoint
	{
		public override bool OnStart()
		{
			RoleEnvironment.Changing += RoleEnvironmentChanging;

			return base.OnStart();
		}

		private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
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
