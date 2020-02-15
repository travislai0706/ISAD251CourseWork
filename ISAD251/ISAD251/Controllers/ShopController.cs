using KungFuTea.Models.Data;
using KungFuTea.Models.ViewModels.Shop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace KungFuTea.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }


        // GET: /shop/category/name
        public ActionResult Category(string name)
        {
            // Declare a list of ProductVM
            List<ItemViewModel> productVMList;

            using (Db db = new Db())
            {
                // Get category id
                Category c = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = c.Id;

                // Init the list
                productVMList = db.Items.ToArray().Where(x => x.CategoryId == catId).Select(x => new ItemViewModel(x)).ToList();

                // Get category name
                var productCat = db.Items.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = productCat.CategoryName;
            }

            // Return view with list
            return View(productVMList);
        }

        // GET: /shop/item-details/name
        [ActionName("item-details")]
        public ActionResult ItemDetails(string name)
        {
            // Declare the view model and item data
            ItemViewModel model;
            Item i;

            // Init item id
            int id = 0;

            using (Db db = new Db())
            {
                // Check if item exists
                if (!db.Items.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                // Init Item
                i = db.Items.Where(x => x.Slug == name).FirstOrDefault();

                // Get id
                id = i.Id;

                // Init model
                model = new ItemViewModel(i);
            }

            // Get gallery images
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images"))
                                                .Select(fn => Path.GetFileName(fn));

            // Return view with model
            return View("ItemDetails", model);
        }
    }
}