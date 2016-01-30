using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class KeyMap : ClassMap<Key>
    {
        public KeyMap()
        {
            Table("`Key`");

            Id(x => x.Id).Column("Id");
            Map(m => m.MajorMinor).Column("MajorMinor");
            Map(m => m.SharpFlatNatural).Column("SharpFlatNatural");
            References(m => m.KeyName).Column("NameId");
        }
    }
}
