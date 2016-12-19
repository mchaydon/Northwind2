using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Northwind.Models;
using Northwind.Security;
using System.Web.Security;
using System.Net;

namespace Northwind.Controllers
{
    public class CustomerController : Controller
    {
        // GET: Customer/Account
        [Authorize]
        public ActionResult Account()
        {
            if (Request.Cookies["role"].Value != "customer")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //ViewBag.CustomerID = UserAccount.GetUserID();
            using (NorthwindEntities db = new NorthwindEntities())
            {
                //find customer using CustomerId (stored in authentication ticket)
                Customer customer = db.Customers.Find(UserAccount.GetUserID());
                //Display original values in textboxes when customer is editing data
                CustomerEdit EditCustomer = new CustomerEdit()
                {
                    CompanyName = customer.CompanyName,
                    ContactName = customer.ContactName,
                    ContactTitle = customer.ContactTitle,
                    Address = customer.Address,
                    City = customer.City,
                    Region = customer.Region,
                    PostalCode = customer.PostalCode,
                    Country = customer.Country,
                    Phone = customer.Phone,
                    Fax = customer.Fax,
                    Email = customer.Email
                };
                return View(EditCustomer);
            }
        }

        // POST: Customer/Account
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Account([Bind(Include = "CompanyName,ContactName,ContactTitle,Address,City,Region,PostalCode,Country,Phone,Fax,Email")] CustomerEdit UpdatedCustomer)
        {
            //for future version, make sure that an authenticated user is a customer
            if (Request.Cookies["role"].Value != "customer")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (NorthwindEntities db = new NorthwindEntities())
            {
                if (ModelState.IsValid)
                {
                    Customer customer = db.Customers.Find(UserAccount.GetUserID());
                    //customer.CompanyName = UpdatedCustomer.CompanyName;
                    //if the customer is changing their CompanyName
                    if (customer.CompanyName.ToLower() != UpdatedCustomer.CompanyName.ToLower())
                    {
                        //Ensure that the CompanyName is unique
                        if (db.Customers.Any(c => c.CompanyName == UpdatedCustomer.CompanyName))
                        {
                            //duplicate CompanyName
                            ModelState.AddModelError("CompanyName", "Duplicate Company Name");
                            return View(UpdatedCustomer);
                        }
                        customer.CompanyName = UpdatedCustomer.CompanyName;
                    }
                    customer.Address = UpdatedCustomer.Address;
                    customer.City = UpdatedCustomer.City;
                    customer.ContactName = UpdatedCustomer.ContactName;
                    customer.ContactTitle = UpdatedCustomer.ContactTitle;
                    customer.Country = UpdatedCustomer.Country;
                    customer.Email = UpdatedCustomer.Email;
                    customer.Fax = UpdatedCustomer.Fax;
                    customer.Phone = UpdatedCustomer.Phone;
                    customer.PostalCode = UpdatedCustomer.PostalCode;
                    customer.Region = UpdatedCustomer.Region;

                    db.SaveChanges();
                    return RedirectToAction(actionName: "Index", controllerName: "Home");
                }
                return View(UpdatedCustomer);
            }
        }

        // GET: Customer/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Customer/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "Email,Password,CompanyName,ContactName,ContactTitle,Address,City,Region,PostalCode,Country,Phone,Fax")] CustomerRegister customerRegister)
        {
            // Add new customer to database
            using (NorthwindEntities db = new NorthwindEntities())
            {
                if (ModelState.IsValid)
                {
                    Customer customer = customerRegister.MapToCustomer();
                    // First, make sure that the customer is unique
                    if (db.Customers.Any(c => c.CompanyName == customer.CompanyName))
                    {
                        // duplicate CompanyName
                        return View();
                    }
                    // Generate guid for this customer
                    customer.UserGuid = System.Guid.NewGuid();
                    // Hash & Salt the customer Password using SHA-1 algorithm
                    customer.Password = UserAccount.HashSHA1(customer.Password + customer.UserGuid);
                    // Save customer to database
                    db.Customers.Add(customer);
                    db.SaveChanges();
                    return RedirectToAction(actionName: "Index", controllerName: "Home");
                }
                // validation error
                return View();
            }
        }

        // GET: Customer/SignIn
        public ActionResult SignIn()
        {
            using (NorthwindEntities db = new NorthwindEntities())
            {
                // create drop-down list box for company name
                ViewBag.CustomerID = new SelectList(db.Customers.OrderBy(c => c.CompanyName),
               "CustomerID", "CompanyName").ToList();
            }
            return View();
        }

        // POST: Customer/SignIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn([Bind(Include = "CustomerId,Password")] CustomerSignIn customerSignIn, string ReturnUrl)
        {
            using (NorthwindEntities db = new NorthwindEntities())
            {
                if (ModelState.IsValid)
                {
                    // find customer by CustomerId
                    Customer customer = db.Customers.Find(customerSignIn.CustomerId);
                    // hash & salt the posted password
                    string str = UserAccount.HashSHA1(customerSignIn.Password + customer.UserGuid);
                    // Compared posted Password to customer password
                    if (str == customer.Password)
                    {
                        // Passwords match
                        // authenticate user (this stores the CustomerID in an encrypted cookie)
                        // normally, you would require HTTPS
                        FormsAuthentication.SetAuthCookie(customer.CustomerID.ToString(), false);

                        //send a cookie to client to indicate that this is a customer
                        HttpCookie myCookie = new HttpCookie("role");
                        myCookie.Value = "customer";
                        Response.Cookies.Add(myCookie);

                        // if there is a return url, redirect to the url
                        if (ReturnUrl != null)
                        {
                            return Redirect(ReturnUrl);
                        }
                        // Redirect to Home page
                        return RedirectToAction(actionName: "Index", controllerName: "Home");
                    }
                    else
                    {
                        // Passwords do not match
                        ModelState.AddModelError("Password", "Incorrect password");
                    }
                    // create drop-down list box for company name
                    ViewBag.CustomerID = new SelectList(db.Customers.OrderBy(c => c.CompanyName),
                   "CustomerID", "CompanyName").ToList();
                    return View();
                }
                // create drop-down list box for company name
                ViewBag.CustomerID = new SelectList(db.Customers.OrderBy(c => c.CompanyName), "CustomerID", "CompanyName").ToList();
                return View();
            }
        }
    }
}