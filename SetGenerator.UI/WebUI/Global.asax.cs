using System;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Ninject;
using Ninject.Web.Common;
using SetGenerator.Data.Repositories;
using SetGenerator.Service;

namespace SetGenerator.WebUI
{
    public class MvcApplication : NinjectHttpApplication
    {
        protected override IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            
            kernel.Load( Assembly.GetExecutingAssembly());

            kernel.Bind<IBandRepository>().To<BandRepository>();
            kernel.Bind<ISongRepository>().To<SongRepository>();
            kernel.Bind<ISetlistRepository>().To<SetlistRepository>();
            kernel.Bind<ISetSongRepository>().To<SetSongRepository>();
            kernel.Bind<IGigRepository>().To<GigRepository>();
            kernel.Bind<IMemberRepository>().To<MemberRepository>();
            kernel.Bind<IInstrumentRepository>().To<InstrumentRepository>();
            kernel.Bind<IValidationRules>().To<ValidationRules>();
            kernel.Bind<IUserRepository>().To<UserRepository>();
            kernel.Bind<ITableColumnRepository>().To<TableColumnRepository>();
            kernel.Bind<IAccount>().To<Account>();

            return kernel;
        }

        protected override void OnApplicationStarted()
        {
            base.OnApplicationStarted();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ControllerBuilder.Current.SetControllerFactory(new NinjectControllerFactory(Kernel));
        }
    }

    public class NinjectControllerFactory : DefaultControllerFactory
    {
        private readonly IKernel _ninjectKernel;
        public NinjectControllerFactory(IKernel kernel)
        {
            _ninjectKernel = kernel;
        }
        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            return (controllerType == null) ? null : (IController)_ninjectKernel.Get(controllerType);
        }
    }
}
