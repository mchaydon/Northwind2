using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Northwind.Models
{
    public class CustomerSignIn
    {
        public int CustomerId { get; set; }
        [Required(ErrorMessage = "Password Required")]
        public string Password { get; set; }
    }
}