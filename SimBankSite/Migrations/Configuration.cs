namespace SimBankSite.Migrations
{
    using SimBankSite.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<SimBankSite.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(SimBankSite.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.
            
            if(!context.Roles.Any(r => r.Name == "Admin"))
            {
                var store = new Microsoft.AspNet.Identity.EntityFramework.RoleStore<Microsoft.AspNet.Identity.EntityFramework.IdentityRole>(context);
                var manager = new Microsoft.AspNet.Identity.RoleManager<Microsoft.AspNet.Identity.EntityFramework.IdentityRole>(store);
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole { Name = "Admin" };

               manager.CreateAsync(role).Wait();
            }

            if (!context.Roles.Any(r => r.Name == "User"))
            {
                var store = new Microsoft.AspNet.Identity.EntityFramework.RoleStore<Microsoft.AspNet.Identity.EntityFramework.IdentityRole>(context);
                var manager = new Microsoft.AspNet.Identity.RoleManager<Microsoft.AspNet.Identity.EntityFramework.IdentityRole>(store);
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole { Name = "User" };

               manager.CreateAsync(role).Wait();
            }

            if (!context.Users.Any(u => u.UserName == "admin@admin.ru"))
            {
                var store = new Microsoft.AspNet.Identity.EntityFramework.UserStore<Models.ApplicationUser>(context);
                var manager = new Microsoft.AspNet.Identity.UserManager<Models.ApplicationUser>(store);
                var user = new ApplicationUser { UserName = "admin@admin.ru" };

                manager.CreateAsync(user, "ebuchayabazadannyh1488").Wait();
                manager.AddToRoleAsync(user.Id, "Admin").Wait();
            }

        }
    }
}
