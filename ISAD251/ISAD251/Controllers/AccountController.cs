using KungFuTea.Models.Data;
using KungFuTea.Models.ViewModels.Account;
using KungFuTea.Models.ViewModels.Shop;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace KungFuTea.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        // GET: /account/login
        [HttpGet]
        public ActionResult Login()
        {
            string UserName = User.Identity.Name;

            if (!string.IsNullOrEmpty(UserName))
                return RedirectToAction("user-profile");

            // Return view
            return View();
        }

        // POST: /account/login
        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if the user is valid

            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.UserName.Equals(model.UserName) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid UserName or password.");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.UserName, model.RememberMe));
            }
        }

        // GET: /account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        // POST: /account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserViewModel model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            // Check if passwords match
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "The password is not match");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                // Make sure username is unique
                if (db.Users.Any(x => x.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", "UserName " + model.UserName + " is taken.");
                    model.UserName = "";
                    return View("CreateAccount", model);
                }

                // Create user
                User user = new User()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    UserName = model.UserName,
                    Password = model.Password
                };

                // Add the data
                db.Users.Add(user);

                // Save
                db.SaveChanges();

                // Add to UserRoles
                int id = user.Id;

                UserRole userRoles = new UserRole()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRoles);
                db.SaveChanges();
            }

            // Create a message
            TempData["SM"] = "You are now registered and can login.";

           //  Redirect
            return Redirect("~/account/login");
        }

        // GET: /account/Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }

        [Authorize]
        public ActionResult UserName()
        {
            // Get username
            string UserName = User.Identity.Name;

            // Declare model
            GetUserNameViewMoel model;

            using (Db db = new Db())
            {
                // Get the user
                User d = db.Users.FirstOrDefault(x => x.UserName == UserName);

                // Build the model
                model = new GetUserNameViewMoel()
                {
                    UserName = d.UserName
                };
            }

            // Return partial view with model
            return PartialView(model);
        }

        // GET: /account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            // Get username
            string UserName = User.Identity.Name;

            // Declare model
            UserProfileViewModel model;

            using (Db db = new Db())
            {
                // Get user
                User d = db.Users.FirstOrDefault(x => x.UserName == UserName);

                // Build model
                model = new UserProfileViewModel(d);
            }

            // Return view
            return View("UserProfile", model);
        }

        // POST: /account/user-profile
        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileViewModel model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            // Check if passwords match if need be
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not match.");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                // Get username
                string UserName = User.Identity.Name;

                // Make sure username is unique
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.UserName == UserName))
                {
                    ModelState.AddModelError("", "UserName " + model.UserName + " already exists.");
                    model.UserName = "";
                    return View("UserProfile", model);
                }

                // Edit database for user
                User d = db.Users.Find(model.Id);

                d.FirstName = model.FirstName;
                d.LastName = model.LastName;
                d.EmailAddress = model.EmailAddress;
                d.UserName = model.UserName;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    d.Password = model.Password;
                }

                // Save
                db.SaveChanges();
            }

            // Set message
            TempData["SM"] = "You have edited your profile!";

            // Redirect
            return Redirect("~/account/user-profile");
        }

        // GET: /account/Orders
        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            // Init list of OrdersForUserVM
            List<OrdersForUserViewModel> ofu = new List<OrdersForUserViewModel>();

            using (Db db = new Db())
            {
                // Get user id
                User u = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                int userId = u.Id;

                // Init list of OrderVM
                List<OrderViewModel> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderViewModel(x)).ToList();

                // Loop through list of OrderVM
                foreach (var order in orders)
                {
                    // Init products dict
                    Dictionary<string, int> ItemsAndQty = new Dictionary<string, int>();

                    // Declare total
                    decimal total = 0m;

                    // Init list of OrderDetailsDTO
                    List<OrderDetails> od = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    // Loop though list of OrderDetailsDTO
                    foreach (var orderDetails in od)
                    {
                        // Get item
                        Item p = db.Items.Where(x => x.Id == orderDetails.ItemId).FirstOrDefault();

                        // Get item price
                        decimal price = p.Price;

                        // Get item name
                        string productName = p.Name;

                        // Add to items 
                        ItemsAndQty.Add(productName, orderDetails.Quantity);

                        // Get total
                        total += orderDetails.Quantity * price;
                    }

                    // Add to orderforuserviewmodel list
                    ofu.Add(new OrdersForUserViewModel()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ItemsAndQty = ItemsAndQty,
                        CreatedDate = order.CreatedDate
                    });
                }

            }

            // Return view
            return View(ofu);
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
    }
}