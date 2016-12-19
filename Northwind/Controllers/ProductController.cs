using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Northwind.Models;

namespace Northwind.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product/Category
        public ActionResult Category()
        {
            // retrieve a list of all categories
            using (NorthwindEntities db = new NorthwindEntities())
            {
                return View(db.Categories.ToList());
            }
        }
        // GET: Product/Discount
        public ActionResult Discount()
        {
            using (NorthwindEntities db = new NorthwindEntities())
            {
                // Filter by date
                DateTime now = DateTime.Now;
                return View(db.Discounts.Where(s => s.StartTime <= now && s.EndTime > now).ToList());
            }
        }

        // GET: Product/Product/1
        public ActionResult Product(int? id)
        {
            // if there is no "category" id, return Http Bad Request
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.id = id;
            using (NorthwindEntities db = new NorthwindEntities())
            {
                // save the selected category name to the ViewBag
                ViewBag.Filter = db.Categories.Find(id).CategoryName;
                // retrieve list of products
                return View(db.Products.Where(p => p.CategoryID == id && p.Discontinued == false).OrderBy(p => p.ProductName).ToList());
            }
        }

        public ActionResult Search()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessSearch(FormCollection Form)
        {
            string search = Form["search"];
            ViewBag.Filter = "Product";
            ViewBag.SearchString = search;
            using (NorthwindEntities db = new NorthwindEntities())
            {
                if (search == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                // retrieve list of products
                return View("Product", db.Products.Where(p => p.ProductName.Contains(search)).ToList());
            }
        }

        // GET: Product/FilterProducts
        public JsonResult FilterProducts(int? id, string SearchString, decimal? PriceFilter)
        {
            using (NorthwindEntities db = new NorthwindEntities())
            {
                // if there is no PriceFilter, return Http Bad Request
                if (PriceFilter == null)
                {
                    Response.StatusCode = 400;
                    return Json(new { }, JsonRequestBehavior.AllowGet);
                }
                var Products = db.Products.Where(p => p.Discontinued == false).ToList();
                if (id != null)
                {
                    Products = Products.Where(p => p.CategoryID == id).ToList();
                }
                if (!String.IsNullOrEmpty(SearchString))
                {
                    Products = Products.Where(p => p.ProductName.Contains(SearchString)).ToList();
                }
                var ProductDTOs = (from p in Products.Where(p => p.UnitPrice > PriceFilter)
                                   orderby p.ProductName
                                   select new
                                   {
                                       p.ProductID,
                                       p.ProductName,
                                       p.QuantityPerUnit,
                                       p.UnitPrice,
                                       p.UnitsInStock
                                   }).ToList();
                return Json(ProductDTOs, JsonRequestBehavior.AllowGet);
            }
        }

    }

}