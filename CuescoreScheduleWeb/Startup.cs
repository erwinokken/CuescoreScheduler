using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CuescoreScheduleWeb.Startup))]
namespace CuescoreScheduleWeb
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
