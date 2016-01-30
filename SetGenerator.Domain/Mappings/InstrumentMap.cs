using FluentNHibernate.Mapping;
using SetGenerator.Domain.Entities;

namespace SetGenerator.Domain.Mappings
{
    public sealed class InstrumentMap : ClassMap<Instrument>
    {
        public InstrumentMap()
        {
            Table("Instrument");

            Id(x => x.Id).Column("Id");
            Map(m => m.Name).Column("Name");
            Map(m => m.Abbreviation).Column("Abbreviation");
        }
    }
}
