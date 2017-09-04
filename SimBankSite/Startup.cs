using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SimBankSite.Startup))]
namespace SimBankSite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            ConfigureAuth(app);
        }
    }
}
