using System.Data.Entity;

namespace SimBankSite.Models
{
    public class OrdersContext : DbContext
    {
        public OrdersContext() : base("name=OrdersContext")
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Service> Services { get; set; }
    }
}