using KungFuTea.Models.Data;
using KungFuTea.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace KungFuTea.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            // create the cart list
            var cart = Session["cart"] as List<CartViewModel> ?? new List<CartViewModel>();

            // Check if cart is empty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty.";
                return View();
            }

            // Declare total and save to ViewBag

            decimal t = 0m;

            foreach (var item in cart)
            {
                t += item.Total;
            }

            ViewBag.GrandTotal = t;

            // Return view
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            // initial CartViewModel
            CartViewModel cmodel = new CartViewModel();

            // initial quantity
            int q = 0;

            // initial price
            decimal p = 0m;

            // Check for cart session
            if (Session["cart"] != null)
            {
                // Get total Quantity and price
                var list = (List<CartViewModel>)Session["cart"];

                foreach (var item in list)
                {
                    q += item.Quantity;
                    p += item.Quantity * item.Price;
                }

                cmodel.Quantity = q;
                cmodel.Price = p;

            }
            else
            {
                // Or set qty and price to 0
                cmodel.Quantity = 0;
                cmodel.Price = 0m;
            }

            // Return partial view
            return PartialView(cmodel);
        }

        public ActionResult AddToCartPartial(int id)
        {
            // Initial CartViewModel list
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel> ?? new List<CartViewModel>();

            // Initial CartViewModel
            CartViewModel cmodel = new CartViewModel();

            using (Db db = new Db())
            {
                // Get the item
                Item i = db.Items.Find(id);

                // Check if the item is already in cart
                var productInCart = cart.FirstOrDefault(x => x.ItemId == id);

                // add new if it is null
                if (productInCart == null)
                {
                    cart.Add(new CartViewModel()
                    {
                        ItemId = i.Id,
                        ItemName = i.Name,
                        Quantity = 1,
                        Price = i.Price,
                        Image = i.ImageName
                    });
                }
                else
                {
                    // increment if exist already
                    productInCart.Quantity++;
                }
            }

            // add total price and quantity to model

            int q = 0;
            decimal p = 0m;

            foreach (var item in cart)
            {
                q += item.Quantity;
                p += item.Quantity * item.Price;
            }

            cmodel.Quantity = q;
            cmodel.Price = p;

            // Save cart back to session
            Session["cart"] = cart;

            // Return partial view
            return PartialView(cmodel);
        }

        // GET: /Cart/AddItem
        public JsonResult AddItem(int itemId)
        {
            // Initial cartlist
            List<CartViewModel> cartlist = Session["cart"] as List<CartViewModel>;

            using (Db db = new Db())
            {
                // Get CartViewModel
                CartViewModel cmodel = cartlist.FirstOrDefault(x => x.ItemId == itemId);

                // Add Quantity
                cmodel.Quantity++;

                var result = new { qty = cmodel.Quantity, price = cmodel.Price };

                // Return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        // GET: /Cart/DropItem
        public ActionResult DropItem (int itemId)
        {
            // Initial cartlist
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel>;

            using (Db db = new Db())
            {
                // Get CartViewModel from list
                CartViewModel model = cart.FirstOrDefault(x => x.ItemId == itemId);

                // Drop Quantity
                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                // Store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                // Return json
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        // GET: /Cart/RemoveItem
        public void RemoveItem(int itemId)
        {
            // Initial cart list
            List<CartViewModel> cartlist = Session["cart"] as List<CartViewModel>;

            using (Db db = new Db())
            {
                // Get CartViewModel from list
                CartViewModel cmodel = cartlist.FirstOrDefault(x => x.ItemId == itemId);

                // Remove cmodel from list
                cartlist.Remove(cmodel);
            }

        }

        // POST: /Cart/CheckOut
        [HttpPost]
        public void CheckOut()
        {
            // Get cart list
            List<CartViewModel> cartlist = Session["cart"] as List<CartViewModel>;

            // Get username
            string UserName = User.Identity.Name;

            int orderId = 0;

            using (Db db = new Db())
            {
                // Initial Order
                Order o = new Order();

                // Get user id
                var q = db.Users.FirstOrDefault(x => x.UserName == UserName);
                int userId = q.Id;

                // Add to Order and save
                o.UserId = userId;
                o.CreatedDate = DateTime.Now;

                db.Orders.Add(o);

                db.SaveChanges();

                // Get inserted id
                orderId = o.OrderId;

                // Initial OrderDetails
                OrderDetails od = new OrderDetails();

                // Add to OrderDetails
                foreach (var item in cartlist)
                {
                    od.OrderId = orderId;
                    od.UserId = userId;
                    od.ItemId = item.ItemId;
                    od.Quantity = item.Quantity;

                    db.OrderDetails.Add(od);

                    db.SaveChanges();
                }
            }
            Session["cart"] = null;
        }
    }

}