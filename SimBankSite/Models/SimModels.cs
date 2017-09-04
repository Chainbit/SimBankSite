using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SimBankSite.Models
{
    public class Sim
    {
        public string Id { get; set; }
        public string TelNumber { get; set; }
    }

    public class SimContext : DbContext
    {
        public SimContext() : base("name=SimContext") { }

        public DbSet<Sim> ActiveSimCards { get; set; }
    }
}