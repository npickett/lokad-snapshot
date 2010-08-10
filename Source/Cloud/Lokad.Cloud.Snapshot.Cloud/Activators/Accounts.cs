#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Collections.Generic;
using Lokad.Cloud.Snapshot.Framework;

namespace Lokad.Cloud.Snapshot.Cloud.Activators
{
	public static class Accounts
	{
		internal static IEnumerable<string> ListAccounts()
		{
			var tokens = CloudEnvironment.GetConfigurationSetting(Names.LiveAccountsConfig).Value.Split(';');
			for (int i = 0; i + 1 < tokens.Length; i += 2)
			{
				yield return tokens[i];
			}
		}

		internal static CloudCredentials GetCredentialsForAccount(string accountName)
		{
			var comparer = StringComparer.OrdinalIgnoreCase;
			var tokens = CloudEnvironment.GetConfigurationSetting(Names.LiveAccountsConfig).Value.Split(';');
			for (int i = 0; i + 1 < tokens.Length; i += 2)
			{
				if (comparer.Equals(accountName, tokens[i]))
				{
					return BuildCredentials(tokens[i], tokens[i + 1]);
				}
			}

			throw new ArgumentException("accountName");
		}

		public static CloudCredentials BuildCredentials(string accountName, string accountKey)
		{
			return new CloudCredentials
				{
					LiveConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", accountName, accountKey),
					SnapshotConnectionString = CloudEnvironment.GetConfigurationSetting(Names.ManagementConnectionStringConfig).Value
				};
		}

		public static CloudCredentials BuildSnapshotOnlyCredentials()
		{
			return new CloudCredentials
			{
				SnapshotConnectionString = CloudEnvironment.GetConfigurationSetting(Names.ManagementConnectionStringConfig).Value
			};
		}
	}
}
