#Fully Automated Snapshots of your Azure Blob Containers and Tables

Although the cloud provides ultra-reliable storage protected against hardware faults, it does not prevent erroneous deletions by the user or faulty application logic. Backup - or rather snapshotting - is therefore vital for cloud apps. Lokad.Snapshot is a .NET application that runs on Windows Azure and backs up Windows Azure Storage (Tables and Blobs) automatically in regular intervals.

Key Features:

* Full snapshots of all blob containers and tables of an Azure account to a dedicated backup account
* Fully automated
* Simple web interface to browse complete snapshots and schedule a full restore, protected by OpenID
* Multiple storage accounts can be backed up using the same dedicated backup account
* Keeps the last N successful snapshots of every account, drops older snapshots
* Can scale up using multiple worker instances if more speed is required
* Provides an endpoint for simple monitoring which is compatible to [Lokad.Monitoring] (http://code.google.com/p/lokad-monitoring/)

Limitations of the current version:

* Only supports tables using the [FatEntity] (https://github.com/Lokad/lokad-cloud/wiki/FatEntities ) type of [Lokad.Cloud] (http://lokad.github.com/lokad-cloud/), i.e. tables which are created and accessed using Lokad.Cloud's table storage provider
* No web interface for configuration, simply edit the !ServiceConfiguration.cscfg file instead.
* No web interface for status reporting. The monitoring REST endpoint is very simple to consume though, or you can deploy [Lokad.Monitoring] (http://code.google.com/p/lokad-monitoring/).
* Azure storage account names can not be longer than 18 characters (Azure would allow 24).

## Getting Started

In [Azure] (http://windows.azure.com) you'll need to create:

* A dedicated storage account. This is where the snapshots will be stored.
* A dedicated hosted service. This is where the snapshot service will be running.

Then you can either checkout the source code and compile it yourself, or simply [download] (https://github.com/Lokad/lokad-snapshot/archives/master) the precompiled Azure package. Once unzipped, you'll see two files:

* Lokad.Cloud.Snapshot.cspkg: This is the packed application ready for deployment to the hosted service
* !ServiceConfiguration.cscfg: The service configuration

Open the service configuration file in a text editor, and configure the the fields accordingly. Please refer to [Configuration] (https://github.com/Lokad/lokad-snapshot/wiki/Configuration) for details. Once configured you can use the [Windows Azure portal] (https://windows.azure.com/) to deploy both files to the hosted service that was created in the beginning.