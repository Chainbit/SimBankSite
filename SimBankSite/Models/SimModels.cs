using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SimBankSite.Models
{
    public class Sim
    {
        /// <summary>
        /// ICCID сим-карты
        /// </summary>
        [Required]
        public string Id { get; set; }
        public string TelNumber { get; set; }
        /// <summary>
        /// Использованные сервисы как массив
        /// </summary>
        [NotMapped]
        public string[] UsedServicesArray
        {
            get
            {
                string[] tab = this.UsedServices.Split(',');
                return tab;
            }
            set
            {
                this.UsedServices = string.Join(",", value);
            }
        }

        /// <summary>
        /// Использованные сервисы как строка (для БД)
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        private string UsedServices { get; set; }
    }

    public class SimContext : DbContext
    {
        public SimContext() : base("Database") { }

        public DbSet<Sim> ActiveSimCards { get; set; }
    }
}