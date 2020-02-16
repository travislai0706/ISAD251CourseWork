using System.Web.Mvc;

namespace KungFuTea.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View("Index");
        }

        // GET: About
        public ActionResult About()
        {
            ViewBag.Message = "About Us";

            return View();
        }

        // GET: Contact
        public ActionResult Contact()
        {
            ViewBag.Message = "Contact Us";

            return View();
        }
    }
}