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
        /// <summary>
        /// Дата создания заказа
        /// </summary>
        public DateTime DateCreated { get; set; }
        /// <summary>
        /// ID клиента
        /// </summary>
        [Required]
        public string CustomerId { get; set; }
        //Order details
        /// <summary>
        /// Выделенный номер телефона
        /// </summary>
        public string TelNumber { get; set; }
        /// <summary>
        /// Статус заказа
        /// </summary>
        public string Status { get; set; }
        public Service Service { get; set; }
        /// <summary>
        /// Ответное сообщение
        /// </summary>
        public string Message { get; set; }
    }

    public class OrderAndService
    {
        public Order Order { get; set; }
        public Service Service { get; set; }
    }
}