using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class SongMap : ClassMap<Song>
    {
        public SongMap()
        {
            Table("Song");

            Id(x => x.Id).Column("Id");
            Map(m => m.Title).Column("Title");
            Map(m => m.Composer).Column("Composer");
            Map(m => m.NeverClose).Column("NeverClose");
            Map(m => m.NeverOpen).Column("NeverOpen");
            Map(m => m.IsDisabled).Column("IsDisabled");
            References(m => m.Genre).Column("GenreId");
            References(m => m.Tempo).Column("TempoId");
            References(m => m.Band).Column("BandId");
            References(m => m.Singer).Column("SingerId");
            References(m => m.Key).Column("KeyId");
            References(m => m.UserCreate).Column("UserCreateId");
            References(m => m.UserUpdate).Column("UserUpdateId");
        }
    }
}
