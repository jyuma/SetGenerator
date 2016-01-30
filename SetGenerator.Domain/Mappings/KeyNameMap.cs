using FluentNHibernate.Mapping;
using SetGenerator.Domain.Entities;

namespace SetGenerator.Domain.Mappings
{
    public sealed class KeyNameMap : ClassMap<KeyName>
    {
        public KeyNameMap()
        {
            Table("KeyName");

            Id(x => x.Id).Column("Id");
            Map(m => m.Name).Column("Name");
        }
    }
}
