using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class TableMap : ClassMap<Table>
    {
        public TableMap()
        {
            Table("`Table`");

            Id(x => x.Id).Column("Id");
            Map(m => m.Name).Column("Name");
        }
    }
}
