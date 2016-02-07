using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class UserPreferenceTableColumnMap : ClassMap<UserPreferenceTableColumn>
    {
        public UserPreferenceTableColumnMap()
        {
            Table("UserPreferenceTableColumn");

            Id(x => x.Id).Column("Id");
            Map(m => m.IsVisible).Column("IsVisible");
            References(m => m.User).Column("UserId");
            References(m => m.Band).Column("BandId");
            References(m => m.TableColumn).Column("TableColumnId");
        }
    }
}
