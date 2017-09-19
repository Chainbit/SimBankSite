using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SimBankSite.Models
{
    public class Service
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }
        [Required]
        [Display(Name ="Название службы")]
        public string Name { get; set; }
        [Display(Name = "Цена")]
        public decimal Price { get; set; }
        [Display(Name = "Номер отправителя СМС")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SenderNumber { get; set; }
    }
}