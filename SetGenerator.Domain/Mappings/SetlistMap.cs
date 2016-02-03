using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class SetlistMap : ClassMap<Setlist>
    {
        public SetlistMap()
        {
            Table("Setlist");

            Id(x => x.Id).Column("Id");
            Map(m => m.Name).Column("Name");
            Map(m => m.DateCreate).Column("DateCreate");
            Map(m => m.DateUpdate).Column("DateUpdate");
            References(m => m.Band).Column("BandId");
            References(m => m.UserCreate).Column("UserCreateId");
            References(m => m.UserUpdate).Column("UserUpdateId");

            HasMany(m => m.SetSongs)
                .Cascade.All()
                .Inverse()
                .Cascade.DeleteOrphan();
        }
    }
}
