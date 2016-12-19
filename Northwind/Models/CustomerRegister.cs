using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Northwind.Models
{
    public class CustomerRegister
    {
        [Required]
        [StringLength(40)]
        [DisplayName("Company Name")]
        public string CompanyName { get; set; }
        [Required]
        [MinLength(4, ErrorMessage = "Password must be at last 4 characters.")]
        public string Password { get; set; }
        [StringLength(30)]
        [DisplayName("Contact Name")]
        public string ContactName { get; set; }
        [StringLength(30)]
        [DisplayName("Contact Title")]
        public string ContactTitle { get; set; }
        [StringLength(60)]
        public string Address { get; set; }
        [StringLength(15)]
        public string City { get; set; }
        [StringLength(10)]
        public string Region { get; set; }
        [StringLength(10)]
        [DisplayName("Postal Code")]
        public string PostalCode { get; set; }
        [StringLength(15)]
        public string Country { get; set; }
        [Phone]
        [StringLength(24)]
        public string Phone { get; set; }
        [Phone]
        [StringLength(24)]
        public string Fax { get; set; }
        [EmailAddress]
        [StringLength(50)]
        public string Email { get; set; }
        public Customer MapToCustomer()
        {
            Customer cust = new Customer()
            {
                Address = this.Address,
                City = this.City,
                CompanyName = this.CompanyName,
                ContactName = this.ContactName,
                ContactTitle = this.ContactTitle,
                Country = this.Country,
                Email = this.Email,
                Fax = this.Fax,
                Phone = this.Phone,
                PostalCode = this.PostalCode,
                Password = this.Password,
                Region = this.Region
            };
            return cust;
        }
    }
}