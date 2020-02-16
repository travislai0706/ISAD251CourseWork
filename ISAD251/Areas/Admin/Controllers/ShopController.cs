using KungFuTea.Models.Data;
using KungFuTea.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PagedList;
using KungFuTea.Areas.Admin.Models.ViewModels.Shop;

namespace KungFuTea.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop/AddItem
        [HttpGet]
        public ActionResult AddItem()
        {
            // Init model
            ItemViewModel model = new ItemViewModel();

            // Add select list of categories to model
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            // Return view with model
            return View(model);
        }

        // POST: Admin/Shop/AddItem
        [HttpPost]
        public ActionResult AddItem(ItemViewModel model, HttpPostedFileBase file)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            // Make sure item name is unique
            using (Db db = new Db())
            {
                if (db.Items.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Please type new name");
                    return View(model);
                }
            }

            // Declare item id
            int id;

            // Init and save item
            using (Db db = new Db())
            {
                Item item = new Item();

                item.Name = model.Name;
                item.Slug = model.Name.Replace(" ", "-").ToLower();
                item.Description = model.Description;
                item.Price = model.Price;
                item.CategoryId = model.CategoryId;

                Category c = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                item.CategoryName = c.Name;

                db.Items.Add(item);
                db.SaveChanges();

                // Get the id
                id = item.Id;
            }

            // Set TempData message
            TempData["SM"] = "You have added a item!";

            #region Upload Image

            // Create necessary directories
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Items");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Items\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Items\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Items\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Items\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            // Check if a file was uploaded
            if (file != null && file.ContentLength > 0)
            {
                // Get file extension
                string ext = file.ContentType.ToLower();

                // Verify extension
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The image was not uploaded.");
                        return View(model);
                    }
                }

                // Init image name
                string imageName = file.FileName;

                // Save image name to DTO
                using (Db db = new Db())
                {
                    Item i = db.Items.Find(id);
                    i.ImageName = imageName;

                    db.SaveChanges();
                }

                // Set original and thumb image paths
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                // Save original
                file.SaveAs(path);

                // Create and save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            // Redirect
            return RedirectToAction("AddItem");
        }

        // GET: Admin/Shop/Items
        public ActionResult Items(int? page, int? catId)
        {
            // Declare a list of ItemViewModel
            List<ItemViewModel> listOfItem;

            // Set page number
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                // Initial the list
                listOfItem = db.Items.ToArray()
                                  .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                  .Select(x => new ItemViewModel(x))
                                  .ToList();

                // Populate categories select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Set selected category
                ViewBag.SelectedCat = catId.ToString();
            }

            // Set pagination
            var onePageOfItems = listOfItem.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfItems = onePageOfItems;

            // Return view with list
            return View(listOfItem);
        }

        // GET: Admin/Shop/EditItem/id
        [HttpGet]
        public ActionResult EditItem(int id)
        {
            // Declare ItemViewModel
            ItemViewModel model;

            using (Db db = new Db())
            {
                // Get the Item
                Item i = db.Items.Find(id);

                // Make sure product exists
                if (i == null)
                {
                    return Content("Item does not exist.");
                }

                // init model
                model = new ItemViewModel(i);

                // Make a select list
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Get all gallery images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images"))
                                                .Select(fn => Path.GetFileName(fn));
            }

            // Return view with model
            return View(model);
        }

        // POST: Admin/Shop/EditItem/id
        [HttpPost]
        public ActionResult EditItem(ItemViewModel model, HttpPostedFileBase file)
        {
            // Get Item id
            int id = model.Id;

            // Populate categories select list and gallery images
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images"))
                                                .Select(fn => Path.GetFileName(fn));

            // Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Items.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "Please type new name");
                    return View(model);
                }
            }

            // Update product
            using (Db db = new Db())
            {
                Item i = db.Items.Find(id);

                i.Name = model.Name;
                i.Slug = model.Name.Replace(" ", "-").ToLower();
                i.Description = model.Description;
                i.Price = model.Price;
                i.CategoryId = model.CategoryId;
                i.ImageName = model.ImageName;

                Category c = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                c.Name = c.Name;

                db.SaveChanges();
            }

            // Set TempData message
            TempData["SM"] = "You have edited the item!";

            #region Image Upload

            // Check for file upload
            if (file != null && file.ContentLength > 0)
            {

                // Get extension
                string ext = file.ContentType.ToLower();

                // Verify extension
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension.");
                        return View(model);
                    }
                }

                // Set uplpad directory paths
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Items\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Items\\" + id.ToString() + "\\Thumbs");

                // Delete files from directories

                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (FileInfo file2 in di1.GetFiles())
                    file2.Delete();

                foreach (FileInfo file3 in di2.GetFiles())
                    file3.Delete();

                // Save image name

                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    Item i = db.Items.Find(id);
                    i.ImageName = imageName;

                    db.SaveChanges();
                }

                // Save original and thumb images

                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            // Redirect
            return RedirectToAction("EditItem");
        }

        // GET: Admin/Shop/DeleteItem/id
        public ActionResult DeleteItem(int id)
        {
            // Delete item from DB
            using (Db db = new Db())
            {
                Item i = db.Items.Find(id);
                db.Items.Remove(i);

                db.SaveChanges();
            }

            // Delete item folder
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            string pathString = Path.Combine(originalDirectory.ToString(), "items\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);

            // Redirect
            return RedirectToAction("items");
        }

        // POST: Admin/Shop/SaveGalleryImages
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            // Loop through files
            foreach (string fileName in Request.Files)
            {
                // Init the file
                HttpPostedFileBase file = Request.Files[fileName];

                // Check it's not null
                if (file != null && file.ContentLength > 0)
                {
                    // Set directory paths
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    // Set image paths
                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                    // Save original and thumb

                    file.SaveAs(path);
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);
                }

            }

        }

        // POST: Admin/Shop/DeleteImage
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }

        // GET: Admin/Shop/Orders
        public ActionResult Orders()
        {
            // Init list of OrdersForAdminVM
            List<OrdersForAdminViewModel> ordersForAdmin = new List<OrdersForAdminViewModel>();

            using (Db db = new Db())
            {
                // Init list of OrderVM
                List<OrderViewModel> orders = db.Orders.ToArray().Select(x => new OrderViewModel(x)).ToList();

                // Loop through list of OrderVM
                foreach (var order in orders)
                {
                    // Init item dictionary
                    Dictionary<string, int> itemsAndQty = new Dictionary<string, int>();

                    // Declare total
                    decimal total = 0m;

                    // Init list of OrderDetailsDTO
                    List<OrderDetails> orderDetailsList = db.OrderDetails.Where(X => X.OrderId == order.OrderId).ToList();

                    // Get username
                    User user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    string username = user.UserName;

                    // Loop through list of OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        // Get item
                        Item i = db.Items.Where(x => x.Id == orderDetails.ItemId).FirstOrDefault();

                        // Get item price
                        decimal price = i.Price;

                        // Get item name
                        string itemtName = i.Name;

                        // Add to item dictionary
                        itemsAndQty.Add(itemtName, orderDetails.Quantity);

                        // Get total
                        total += orderDetails.Quantity * price;
                    }

                    // Add to OrdersForAdminViewModel
                    ordersForAdmin.Add(new OrdersForAdminViewModel()
                    {
                        OrderNumber = order.OrderId,
                        UserName = username,
                        Total = total,
                        ItemsAndQty = itemsAndQty,
                        CreatedDate = order.CreatedDate
                    });
                }
            }

            // Return view with OrdersForAdminVM list
            return View(ordersForAdmin);
        }

        public ActionResult DeleteOrder(int Id)
        {
            using (Db db = new Db())
            {
                Order od = db.Orders.Find(Id);
                db.Orders.Remove(od);

                db.SaveChanges();
            }

            // Redirect
            return RedirectToAction("Orders");
        }
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            // Declare a list of models
            List<CategoryViewModel> category;

            using (Db db = new Db())
            {
                // Init the list
                category = db.Categories
                                .ToArray()
                                .OrderBy(x => x.Sorting)
                                .Select(x => new CategoryViewModel(x))
                                .ToList();
            }

            // Return view with list
            return View(category);
        }


        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                // Set initial count
                int count = 1;

                // Declare CategoryDTO
                Category c;

                // Set sorting for each category
                foreach (var catId in id)
                {
                    c = db.Categories.Find(catId);
                    c.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }

        }

        // GET: Admin/Shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                // Get the category
                Category c = db.Categories.Find(id);

                // Remove the category
                db.Categories.Remove(c);

                // Save
                db.SaveChanges();
            }

            // Redirect
            return RedirectToAction("Categories");
        }

        // POST: Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                // Check category name is unique
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                // Get Category
                Category c = db.Categories.Find(id);

                // Edit Category
                c.Name = newCatName;
                c.Slug = newCatName.Replace(" ", "-").ToLower();

                // Save
                db.SaveChanges();
            }

            // Return
            return "ok";
        }

    }
}