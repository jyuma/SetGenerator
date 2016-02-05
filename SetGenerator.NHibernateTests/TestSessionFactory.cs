using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using SetGenerator.Domain.Mappings;

namespace SetGenerator.NHibernateTests
{
    public class TestSessionFactory
    {
        private static ISessionFactory SessionFactory
        {
            get
            {
                return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2012
                .ConnectionString(c => c.FromConnectionStringWithKey("NHibernateConnectionString")))
                .Mappings(m => m.FluentMappings.Add<BandMap>())
                .Mappings(m => m.FluentMappings.Add<GenreMap>())
                .Mappings(m => m.FluentMappings.Add<GigMap>())
                .Mappings(m => m.FluentMappings.Add<InstrumentMap>())
                .Mappings(m => m.FluentMappings.Add<KeyMap>())
                .Mappings(m => m.FluentMappings.Add<KeyNameMap>())
                .Mappings(m => m.FluentMappings.Add<MemberInstrumentMap>())
                .Mappings(m => m.FluentMappings.Add<MemberMap>())
                .Mappings(m => m.FluentMappings.Add<SetlistMap>())
                .Mappings(m => m.FluentMappings.Add<SetSongMap>())
                .Mappings(m => m.FluentMappings.Add<SongMap>())
                .Mappings(m => m.FluentMappings.Add<SongMemberInstrumentMap>())
                .Mappings(m => m.FluentMappings.Add<TableColumnMap>())
                .Mappings(m => m.FluentMappings.Add<TableMap>())
                .Mappings(m => m.FluentMappings.Add<TempoMap>())
                .Mappings(m => m.FluentMappings.Add<UserMap>())
                .Mappings(m => m.FluentMappings.Add<UserBandMap>())
                .Mappings(m => m.FluentMappings.Add<UserPreferenceTableColumnMap>())
                .Mappings(m => m.FluentMappings.Add<UserPreferenceTableMemberMap>())
                .Mappings(m => m.FluentMappings.Add<SongMemberInstrumentMatchMap>())
                
                .BuildSessionFactory();
            }
        }
        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }

    }
}

