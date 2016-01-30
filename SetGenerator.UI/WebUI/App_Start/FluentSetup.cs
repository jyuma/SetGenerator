using SetGenerator.Domain.Mappings;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using Ninject.Web.Common;

namespace SetGenerator.WebUI
{
    public class NHibernateSessionFactory
    {
        public ISessionFactory GetSessionFactory()
        {
            ISessionFactory fluentConfiguration = Fluently.Configure()
                                                     .Database(MsSqlConfiguration.MsSql2012.ConnectionString(c => c.FromConnectionStringWithKey("NHibernateConnectionString")))
                                                     .Mappings(m => m.FluentMappings.AddFromAssemblyOf<UserMap>())
                                                     .ExposeConfiguration(BuidSchema)
                                                     .BuildSessionFactory();
            return fluentConfiguration;
        }

        //WARNING THIS WILL TRY TO DROP ALL YOUR TABLES EVERYTIME YOU LAUNCH YOUR SITE. DISABLE AFTER CREATING DATABASE.
        private static void BuidSchema(NHibernate.Cfg.Configuration config)
        {
            //new NHibernate.Tool.hbm2ddl.SchemaExport(config).Create(false, true);
        }
    }

    public class NhibernateModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISessionFactory>().ToProvider<NhibernateSessionFactoryProvider>().InSingletonScope();
            Bind<ISession>().ToMethod(context => context.Kernel.Get<ISessionFactory>().OpenSession()).InRequestScope();
        }
    }

    public class NhibernateSessionFactoryProvider : Provider<ISessionFactory>
    {
        protected override ISessionFactory CreateInstance(IContext context)
        {
            var sessionFactory = new NHibernateSessionFactory();
            return sessionFactory.GetSessionFactory();
        }
    }
}