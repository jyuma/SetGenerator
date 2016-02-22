using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class MemberMap : ClassMap<Member>
    {
        public MemberMap()
        {
            Table("Member");

            Id(x => x.Id).Column("Id");
            Map(m => m.FirstName).Column("FirstName");
            Map(m => m.LastName).Column("LastName");
            Map(m => m.Alias).Column("Alias");

            References(m => m.DefaultInstrument).Column("DefaultInstrumentId").Nullable();
            References(m => m.Band).Column("BandId");

            HasMany(x => x.MemberInstruments)
                .Cascade.All()
                .Inverse()
                .Cascade.DeleteOrphan();
        }
    }
}
