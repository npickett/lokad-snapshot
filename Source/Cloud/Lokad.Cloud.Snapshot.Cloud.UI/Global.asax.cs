using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Lokad.Cloud.Snapshot.Cloud.UI
{
	public class MvcApplication : HttpApplication
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.MapRoute("Default", "{controller}/{action}/{account}/{id}", new { controller = "Snapshots", action = "Index", account = UrlParameter.Optional, id = UrlParameter.Optional });
		}

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);
		}
	}
}