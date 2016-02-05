using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class SongMemberInstrumentMatchMap : ClassMap<SongMemberInstrumentMatch>
    {
        public SongMemberInstrumentMatchMap()
        {
            Table("vwSongMemberInstrumentMatch");

            Id(m => m.Id).Column("Id");
            References(m => m.Band).Column("BandId");
            References(m => m.Song).Column("SongId");
            References(m => m.MatchingSong).Column("MatchingSongId");
        }
    }
}
