using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class TempoMap : ClassMap<Tempo>
    {
        public TempoMap()
        {
            Table("Tempo");

            Id(x => x.Id).Column("Id");
            Map(m => m.Name).Column("Name");
        }
    }
}
