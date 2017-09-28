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

        [DisplayName("Номер")]
        public string TelNumber { get; set; }

        //[JsonIgnore]
        //public decimal Balance { get; set; }

        /// <summary>
        /// Использованные сервисы как строка (для БД)
        /// </summary>
        [JsonIgnore]
        [DisplayName("Использованные сервисы")]
        public string UsedServices { get; set; }

        [DisplayName("ID компьютера")]
        public string SimBankId { get; set; }

        /// <summary>
        /// Состояние сим карты (готов или используется)
        /// </summary>
        [DisplayName("Состояние")]
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

    public enum SimState
    {
        Ready,
        InUse,
        Disconnected
    }
}