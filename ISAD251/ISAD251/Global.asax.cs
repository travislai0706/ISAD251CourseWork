using KungFuTea.Models.Data;
using System.Linq;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace KungFuTea
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AuthenticateRequest()
        {
            // Check if user is logged in
            if (User == null) { return; }

            // Get username
            string UserName = Context.User.Identity.Name;

            // Declare array of roles
            string[] roles = null;

            using (Db db = new Db())
            {
                // Populate roles
                User u = db.Users.FirstOrDefault(x => x.UserName == UserName);

                roles = db.UserRoles.Where(x => x.UserId == u.Id).Select(x => x.Role.Name).ToArray();
            }

            // Build IPrincipal object
            IIdentity userIdentity = new GenericIdentity(UserName);
            IPrincipal newUserObj = new GenericPrincipal(userIdentity, roles);

            // Update Context.User
            Context.User = newUserObj;
        }
    }
}
