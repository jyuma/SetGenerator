using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class MemberInstrumentMap : ClassMap<MemberInstrument>
    {
        public MemberInstrumentMap()
        {
            Table("MemberInstrument");

            Id(x => x.Id).Column("Id");
            References(m => m.Band).Column("BandId");
            References(m => m.Member).Column("MemberId");
            References(m => m.Instrument).Column("InstrumentId");
        }
    }
}
