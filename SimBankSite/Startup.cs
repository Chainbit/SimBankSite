using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(SimBankSite.Startup))]
namespace SimBankSite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
            var configuration = new Migrations.Configuration();
            var migrator = new System.Data.Entity.Migrations.DbMigrator(configuration);
            migrator.Update();
        }
    }
}
