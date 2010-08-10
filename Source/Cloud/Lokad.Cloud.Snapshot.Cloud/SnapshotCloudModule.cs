#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System.Collections.Generic;
using Autofac.Builder;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Snapshot.Cloud.Activators;
using Lokad.Cloud.Snapshot.Cloud.Handlers;
using Lokad.Cloud.Snapshot.Cloud.Mapping;
using Lokad.Cloud.Snapshot.Cloud.Publishing;
using Lokad.Cloud.Snapshot.Framework;

namespace Lokad.Cloud.Snapshot.Cloud
{
	public class SnapshotCloudModule : Module 
	{
		protected override void Load(ContainerBuilder builder)
		{
			var providers = Standalone.CreateProviders(CloudEnvironment.GetConfigurationSetting(Names.ManagementConnectionStringConfig).Value);

			builder.Register(providers).SingletonScoped().ExternallyOwned();
			builder.Register(providers.Log).SingletonScoped().ExternallyOwned();

			builder.Register<SnapshotPublisher>().FactoryScoped();
			builder.Register<RestorePublisher>().FactoryScoped();

			builder.Register<UniqueNamingScheme>().As<IContainerNameMappingScheme>().FactoryScoped();

			// Cloud Services:

			builder.RegisterCollection<CloudService>().As<IEnumerable<CloudService>>();

			RegisterCloudService<SnapshotService>(builder);
			RegisterCloudService<RestoreService>(builder);
			RegisterCloudService<SnapshotBlobService>(builder);
			RegisterCloudService<RestoreBlobService>(builder);
			RegisterCloudService<SnapshotTableService>(builder);
			RegisterCloudService<RestoreTableService>(builder);
			RegisterCloudService<DeleteSnapshotService>(builder);

			RegisterCloudService<AutoQueueNewSnapshotsService>(builder);
			RegisterCloudService<AutoQueueOldSnapshotsForRemovalService>(builder);
			RegisterCloudService<AutoPruneOldReportsService>(builder);
		}

		static void RegisterCloudService<TService>(ContainerBuilder builder)
		{
			builder.Register<TService>()
				.As<CloudService>()
				.OnActivating(ActivatingHandler.InjectUnsetProperties)
				.FactoryScoped()
				.ExternallyOwned()
				.MemberOf<IEnumerable<CloudService>>();
		}
	}
}
