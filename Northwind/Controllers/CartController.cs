using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Northwind.Models;
using Northwind.Security;


namespace Northwind.Controllers
{
    public class CartController : Controller
    {
        // POST: Cart/AddToCart
        [HttpPost]
        public JsonResult AddToCart(CartDTO cartDTO)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
            return Json(new { }, JsonRequestBehavior.AllowGet);
            }
            // create cart item from Json object
            Cart sc = new Cart();
            sc.ProductID = cartDTO.ProductID;
            sc.CustomerID = cartDTO.CustomerID;
            sc.Quantity = cartDTO.Quantity;
            using (NorthwindEntities db = new NorthwindEntities())
            {
                // if there is a duplicate product id in cart, simply update the quantity
                if (db.Carts.Where(c => c.ProductID == sc.ProductID && c.CustomerID ==
               sc.CustomerID).Any())
                {
                    // this product is already in the customer's cart,
                    // update the existing cart item's quantity
                    Cart cart = db.Carts.Where(c => c.ProductID == sc.ProductID && c.CustomerID ==
                    sc.CustomerID).FirstOrDefault();
                    cart.Quantity += sc.Quantity;
                    sc = new Cart()
                    {
                        CartID = cart.CartID,
                        ProductID = cart.ProductID,
                        CustomerID = cart.CustomerID,
                        Quantity = cart.Quantity
                    };
                }
                else
                {
                    // this product is not in the customer's cart, add the product
                    db.Carts.Add(sc);
                }
                db.SaveChanges();
            }
            return Json(sc, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateCartQuantity(CartDTO cartDTO)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
            // create cart item from Json object
            Cart sc = new Cart();
            sc.ProductID = cartDTO.ProductID;
            sc.CustomerID = cartDTO.CustomerID;
            sc.Quantity = cartDTO.Quantity;
            using (NorthwindEntities db = new NorthwindEntities())
            {
                // this product is already in the customer's cart,
                // update the existing cart item's quantity
                Cart cart = db.Carts.Where(c => c.ProductID == sc.ProductID && c.CustomerID == sc.CustomerID).FirstOrDefault();
                cart.Quantity = sc.Quantity;
                sc = new Cart()
                {
                    CartID = cart.CartID,
                    ProductID = cart.ProductID,
                    CustomerID = cart.CustomerID,
                    Quantity = cart.Quantity
                };
                db.SaveChanges();
            }
            return Json(sc, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RemoveFromCart(CartDTO cartDTO)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
            // create cart item from Json object
            Cart sc = new Cart();
            sc.ProductID = cartDTO.ProductID;
            sc.CustomerID = cartDTO.CustomerID;
            using (NorthwindEntities db = new NorthwindEntities())
            {
                // this product is already in the customer's cart,
                // update the existing cart item's quantity
                Cart cart = db.Carts.Where(c => c.ProductID == sc.ProductID && c.CustomerID ==
                sc.CustomerID).FirstOrDefault();
                db.Carts.Remove(cart);
                db.SaveChanges();
            }
            return Json(sc, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubmitOrder()
        {   
            using (NorthwindEntities db = new NorthwindEntities())
            {
                Customer customer = db.Customers.Find(UserAccount.GetUserID());
                List<Cart> cart = db.Carts.Include("Product").Include("Customer").Where(c => c.CustomerID == customer.CustomerID).ToList();
                Order order = new Order();
                order.CustomerID = cart[0].CustomerID;
                order.EmployeeID = 10;
                order.OrderDate = DateTime.Now;
                order.ShipName = cart[0].Customer.CompanyName;
                order.ShipAddress = cart[0].Customer.Address;
                order.ShipCity = cart[0].Customer.City;
                order.ShipRegion = cart[0].Customer.Region;
                order.ShipPostalCode = cart[0].Customer.PostalCode;
                order.ShipCountry = cart[0].Customer.Country;
                db.Orders.Add(order);

                db.SaveChanges();

                List<Order> ord = db.Orders.ToList();

                var orderID = ord.ToList().Last().OrderID;
                
                foreach (Cart c in cart) 
                {
                    c.Product.UnitsInStock -= Convert.ToInt16(c.Quantity);
                    OrderDetail orderDetail = new OrderDetail();
                    orderDetail.OrderID = orderID;
                    orderDetail.ProductID = Convert.ToInt32(c.ProductID);
                    orderDetail.UnitPrice = Convert.ToDecimal(c.Product.UnitPrice);
                    orderDetail.Quantity = Convert.ToInt16(c.Quantity);

                    db.OrderDetails.Add(orderDetail);
                    db.SaveChanges();

                }

                

            }


                return View();
        }

        //public JsonResult SumbitOrder(CartDTO cartDTO)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        Response.StatusCode = 400;
        //        return Json(new { }, JsonRequestBehavior.AllowGet);
        //    }
        //    // create cart item from Json object
        //    OrderDetail od = new OrderDetail();
        //    Cart sc = new Cart();
        //    sc.ProductID = cartDTO.ProductID;
        //    sc.CustomerID = cartDTO.CustomerID;
        //    sc.Quantity = cartDTO.Quantity;
        //    using (NorthwindEntities db = new NorthwindEntities())
        //    {
        //        // remove all items from the cart where customerID equals submitted customerID
        //        List<Cart> cart = db.Carts.Include("Product").Include("Customer").Where(c => c.CustomerID == sc.CustomerID).ToList();
        //        Order order = new Order();
        //        order.CustomerID = sc.CustomerID;

        //        foreach (Cart c in cart)
        //        {
        //            c.Product.UnitsInStock -= Convert.ToInt16(sc.Quantity);
        //            db.Carts.Remove(c);
        //        }
        //        db.Orders.Add(order);   
        //        db.SaveChanges();
        //    }
        //    return Json(sc, JsonRequestBehavior.AllowGet);
        //}



        public ActionResult Cart()
        {
            using (NorthwindEntities db = new NorthwindEntities())
            {
                Customer customer = db.Customers.Find(UserAccount.GetUserID());
                var getCart = db.Carts.Include("Product").Include("Customer").Where(c => c.CustomerID == customer.CustomerID).ToList();

                return View(getCart);

            }
        }
    }
}