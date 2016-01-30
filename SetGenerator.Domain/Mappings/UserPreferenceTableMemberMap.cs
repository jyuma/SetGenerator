using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class UserPreferenceTableMemberMap : ClassMap<UserPreferenceTableMember>
    {
        public UserPreferenceTableMemberMap()
        {
            Table("UserPreferenceTableMember");

            Id(x => x.Id).Column("Id");
            Map(m => m.IsVisible).Column("IsVisible");
            References(m => m.Table).Column("TableId");
            References(m => m.Band).Column("BandId");
            References(m => m.User).Column("UserId");
        }
    }
}
