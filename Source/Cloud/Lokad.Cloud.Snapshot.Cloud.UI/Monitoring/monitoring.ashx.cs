using System;
using System.Linq;
using System.Web;
using Lokad.Cloud.Snapshot.Cloud.Reports;
using Lokad.Cloud.Storage;

namespace Lokad.Cloud.Snapshot.Cloud.UI.Monitoring
{
	/// <summary>Really Simple Monitoring endpoint.</summary>
	/// <remarks>This class grabs data to be pushed through the monitoring endpoint.</remarks>
	public class Monitoring : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentType = "application/xml";

			var query = HttpContext.Current.Request.Url.Query;
			if (!query.Contains(CloudEnvironment.GetConfigurationSetting("MonitoringApiKey").Value))
			{
				context.Response.StatusCode = 403; // access forbidden
				context.Response.Write("You do not have access to the monitoring endpoint.");
				return;
			}

			try
			{
				var doc = new RsmDocument
					{
						Indicators = Table<MonitoringIndicatorReport>(Names.IndicatorReportsTable).Get().Select(e => e.Value).ToArray(),
						Messages = Table<MonitoringMessageReport>(Names.MessageReportsTable).Get().Select(e => e.Value).ToArray()
					};

				context.Response.Write(doc.ToString());
			}
			catch (Exception ex)
			{
				// helper to facilitate troubleshooting the endpoint if needed
				context.Response.Write(ex.ToString());
			}
		}

		public bool IsReusable
		{
			get { return true; }
		}

		private static CloudTable<T> Table<T>(string name)
		{
			return new CloudTable<T>(GlobalSetup.Container.Resolve<CloudInfrastructureProviders>().TableStorage, name);
		}
	}
}