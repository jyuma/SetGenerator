using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class BandMap : ClassMap<Band>
    {
        public BandMap()
        {
            Table("Band");

            Id(x => x.Id).Column("Id");
            Map(m => m.Name).Column("Name");
            References(m => m.DefaultSinger).Column("DefaultSingerId");

            HasMany(m => m.Members).Cascade.All(); ;
            HasMany(m => m.Songs).Cascade.All(); ;
            HasMany(m => m.Gigs).Cascade.All(); ;
        }
    }
}
