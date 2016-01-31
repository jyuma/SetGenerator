using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class SetMap : ClassMap<Set>
    {
        public SetMap()
        {
            Table("Set");

            Id(x => x.Id).Column("Id");
            Map(m => m.Number).Column("Number");
            Map(m => m.Sequence).Column("Sequence");
            References(m => m.Setlist).Column("SetlistId");
            References(m => m.Song).Column("SongId");

            HasMany(m => m.Songs).Cascade.All(); ;
        }
    }
}
