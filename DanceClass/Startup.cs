using Autofac;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DanceClass.Startup))]
namespace DanceClass
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public void ConfigureAutofac(IAppBuilder app)
        {
            var builder = new ContainerBuilder();
        }
    }
}
