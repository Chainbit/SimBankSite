namespace SimBankSite.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
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
            
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));
            var manager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
               
                var role = new IdentityRole { Name = "Admin" };

               manager.CreateAsync(role).Wait();
            }

            if (!context.Roles.Any(r => r.Name == "User"))
            {
                
                var role = new IdentityRole { Name = "User" };

               manager.CreateAsync(role).Wait();
            }

            if (!context.Users.Any(u => u.UserName == "admin@admin.ru"))
            {
              
                var user = new ApplicationUser { UserName = "admin@admin.ru"};

                userManager.CreateAsync(user, "ebuchayabazadannyh1488").Wait();
                userManager.AddToRoleAsync(user.Id, "Admin").Wait();
            }

        }
    }
}
