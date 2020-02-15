﻿using KungFuTea.Models.Data;
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
            // Init the cart list
            var cart = Session["cart"] as List<CartViewModel> ?? new List<CartViewModel>();

            // Check if cart is empty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty.";
                return View();
            }

            // Calculate total and save to ViewBag

            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            // Return view with list
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            // Init CartVM
            CartViewModel model = new CartViewModel();

            // Init quantity
            int qty = 0;

            // Init price
            decimal price = 0m;

            // Check for cart session
            if (Session["cart"] != null)
            {
                // Get total qty and price
                var list = (List<CartViewModel>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;

            }
            else
            {
                // Or set qty and price to 0
                model.Quantity = 0;
                model.Price = 0m;
            }

            // Return partial view with model
            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            // Init CartVM list
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel> ?? new List<CartViewModel>();

            // Init CartVM
            CartViewModel model = new CartViewModel();

            using (Db db = new Db())
            {
                // Get the product
                Item p = db.Items.Find(id);

                // Check if the product is already in cart
                var productInCart = cart.FirstOrDefault(x => x.ItemId == id);

                // If not, add new
                if (productInCart == null)
                {
                    cart.Add(new CartViewModel()
                    {
                        ItemId = p.Id,
                        ItemName = p.Name,
                        Quantity = 1,
                        Price = p.Price,
                        Image = p.ImageName
                    });
                }
                else
                {
                    // If it is, increment
                    productInCart.Quantity++;
                }
            }

            // Get total qty and price and add to model

            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            // Save cart back to session
            Session["cart"] = cart;

            // Return partial view with model
            return PartialView(model);
        }

        // GET: /Cart/IncrementProduct
        public JsonResult IncrementProduct(int itemId)
        {
            // Init cart list
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel>;

            using (Db db = new Db())
            {
                // Get cartVM from list
                CartViewModel model = cart.FirstOrDefault(x => x.ItemId == itemId);

                // Increment qty
                model.Quantity++;

                // Store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                // Return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        // GET: /Cart/DecrementProduct
        public ActionResult DecrementProduct(int itemId)
        {
            // Init cart
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel>;

            using (Db db = new Db())
            {
                // Get model from list
                CartViewModel model = cart.FirstOrDefault(x => x.ItemId == itemId);

                // Decrement qty
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

        // GET: /Cart/RemoveProduct
        public void RemoveProduct(int itemId)
        {
            // Init cart list
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel>;

            using (Db db = new Db())
            {
                // Get model from list
                CartViewModel model = cart.FirstOrDefault(x => x.ItemId == itemId);

                // Remove model from list
                cart.Remove(model);
            }

        }

        public ActionResult PaypalPartial()
        {
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel>;

            return PartialView(cart);
        }

        // POST: /Cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            // Get cart list
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel>;

            // Get username
            string UserName = User.Identity.Name;

            int orderId = 0;

            using (Db db = new Db())
            {
                // Init OrderDTO
                Order o = new Order();

                // Get user id
                var q = db.Users.FirstOrDefault(x => x.UserName == UserName);
                int userId = q.Id;

                // Add to OrderDTO and save
                o.UserId = userId;
                o.CreatedDate = DateTime.Now;

                db.Orders.Add(o);

                db.SaveChanges();

                // Get inserted id
                orderId = o.OrderId;

                // Init OrderDetailsDTO
                OrderDetails od = new OrderDetails();

                // Add to OrderDetailsDTO
                foreach (var item in cart)
                {
                    od.OrderId = orderId;
                    od.UserId = userId;
                    od.ItemId = item.ItemId;
                    od.Quantity = item.Quantity;

                    db.OrderDetails.Add(od);

                    db.SaveChanges();
                }
            }
        }
    }
}