using System.Data.Entity;

namespace SimBankSite.Models
{
    public class ServiceContext : DbContext
    {
        public ServiceContext() : base("ServiceContext")
        {
        }

        public DbSet<Order> Orders { get; set; }
        //public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Service> Services { get; set; }
    }
}