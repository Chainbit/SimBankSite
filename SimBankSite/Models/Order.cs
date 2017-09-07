using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SimBankSite.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        [Required]
        public string CustomerId { get; set; }
        //Order details
        public string TelNumber { get; set; }
        public string Status { get; set; }
        public Service Service { get; set; }
        public string Message { get; set; }
    }
}