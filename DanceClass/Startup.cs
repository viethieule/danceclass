using Autofac;
using Autofac.Integration.Mvc;
using DataAccess;
using DataAccess.Entities;
using DataAccess.IdentityAccessor;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using Owin;
using Services.Common.AutoMapper;
using Services.Members;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

[assembly: OwinStartupAttribute(typeof(DanceClass.Startup))]
namespace DanceClass
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            ConfigureAutofac(app);
        }

        public void ConfigureAutofac(IAppBuilder app)
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new AutoMapperModule(typeof(AutoMapperModule).Assembly));
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            builder.RegisterType<DanceClassDbContext>().AsSelf().InstancePerRequest();

            // Asp.net identity
            builder.RegisterType<ApplicationUserManager>().AsSelf().InstancePerRequest();
            builder.RegisterType<ApplicationSignInManager>().AsSelf().InstancePerRequest();
            builder.RegisterType<ApplicationUserStore>().As<IUserStore<ApplicationUser, int>>().InstancePerRequest();
            builder.Register(c => HttpContext.Current.GetOwinContext().Authentication).InstancePerRequest();
            builder.Register(c => app.GetDataProtectionProvider()).InstancePerRequest();

            builder.RegisterAssemblyTypes(typeof(MemberService).Assembly)
                .Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces()
                .InstancePerRequest();

            IContainer container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
