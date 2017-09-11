using Newtonsoft.Json;
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
    /// <summary>
    /// Класс представляющий собой активную сим-карту
    /// </summary>
    public class ActiveSim : Sim
    {
        public string SimBankId { get; set; }
        /// <summary>
        /// Состояние сим карты (готов или используется)
        /// </summary>
        public SimState State { get; set; }

        /// <summary>
        /// Использованные сервисы как массив
        /// </summary>
        [NotMapped]
        [JsonIgnore]
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
    }


    /// <summary>
    /// Базовый класс представляющий сим-карту
    /// </summary>
    [Serializable]
    public class Sim
    {
        [Required]
        /// <summary>
        /// ICCID сим-карты
        /// </summary>
        public string Id { get; set; }
        public string TelNumber { get; set; }


        /// <summary>
        /// Использованные сервисы как строка (для БД)
        /// </summary>
        [JsonIgnore]
        public string UsedServices { get; set; }
    }

    //public class SimContext : DbContext
    //{
    //    public SimContext() : base("Database") { }

    //    /// <summary>
    //    /// Активные сим-карты
    //    /// </summary>
    //    public DbSet<ActiveSim> ActiveSimCards { get; set; }
    //}

    //public class SimStorageContext : DbContext
    //{
    //    public SimStorageContext(): base("Database") { }

    //    /// <summary>
    //    /// Все сим-карты
    //    /// </summary>
    //    public DbSet<Sim> AllSimCards { get; set; }
    //}

    public enum SimState
    {
        Ready,
        InUse
    }
}