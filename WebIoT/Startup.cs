using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebIoT.Startup))]
namespace WebIoT
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
