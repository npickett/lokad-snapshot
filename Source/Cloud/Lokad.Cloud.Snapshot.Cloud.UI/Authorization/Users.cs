#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Lokad.Cloud.Snapshot.Cloud.UI.Authorization
{
	public class Users
	{
		public string Identifier { get; private set; }

		public static bool IsAdministrator(string identifier)
		{
			var admins = CloudEnvironment.IsAvailable
				? RoleEnvironment.GetConfigurationSettingValue("Admins")
				: ConfigurationManager.AppSettings["Admins"];

			return admins
				.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
				.Contains(identifier);
		}

		public static IEnumerable<Users> GetAdministrators()
		{
			var admins = CloudEnvironment.IsAvailable
				? RoleEnvironment.GetConfigurationSettingValue("Admins")
				: ConfigurationManager.AppSettings["Admins"];

			return admins
				.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
				.Select(admin => new Users
				{
					Identifier = admin
				});
		}
	}
}