using System.Web.Mvc;
using System.Web.Routing;

namespace KungFuTea
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Account", "Account/{action}/{id}", new { controller = "Account", action = "Index", id = UrlParameter.Optional }, new[] { "KungFuTea.Controllers" });

            routes.MapRoute("Cart", "Cart/{action}/{id}", new { controller = "Cart", action = "Index", id = UrlParameter.Optional }, new[] { "KungFuTea.Controllers" });

            routes.MapRoute("Shop", "Shop/{action}/{name}", new { controller = "Shop", action = "Index", name = UrlParameter.Optional }, new[] { "KungFuTea.Controllers" });

            routes.MapRoute("Home", "Home/{action}/{name}", new { controller = "Home", action = "Index", name = UrlParameter.Optional }, new[] { "KungFuTea.Controllers" });
            routes.MapRoute("Pages", "{page}", new { controller = "Pages", action = "Index" }, new[] { "KungFuTea.Controllers" });
            routes.MapRoute("Default", "", new { controller = "Home", action = "Index" }, new[] { "KungFuTea.Controllers" });

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}
