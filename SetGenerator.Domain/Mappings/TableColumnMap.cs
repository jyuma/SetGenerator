using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class TableColumnMap : ClassMap<TableColumn>
    {
        public TableColumnMap()
        {
            Table("TableColumn");

            Id(x => x.Id).Column("Id");
            Map(m => m.Name).Column("Name");
            Map(m => m.Data).Column("Data");
            Map(m => m.AlwaysVisible).Column("AlwaysVisible");
            Map(m => m.Sequence).Column("Sequence");
            References(m => m.Table).Column("TableId");
        }
    }
}
