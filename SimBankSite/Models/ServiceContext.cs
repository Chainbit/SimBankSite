using System.Data.Entity;

namespace SimBankSite.Models
{
    public class ServiceContext : DbContext
    {
        public ServiceContext() : base("Database")
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Service> Services { get; set; }
    }
}