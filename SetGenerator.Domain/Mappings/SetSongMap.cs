using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class SetSongMap : ClassMap<SetSong>
    {
        public SetSongMap()
        {
            Table("SetSong");

            Id(x => x.Id).Column("Id");
            Map(m => m.SetNumber).Column("SetNumber");
            Map(m => m.Sequence).Column("Sequence");
            References(m => m.Setlist).Column("SetlistId");
            References(m => m.Song).Column("SongId");
        }
    }
}
