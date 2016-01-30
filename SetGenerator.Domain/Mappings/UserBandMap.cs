using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class UserBandMap : ClassMap<UserBand>
    {
        public UserBandMap()
        {
            Table("UserBand");

            Id(x => x.Id).Column("Id");

            References(m => m.User).Column("UserId");
            References(m => m.Band).Column("BandId");
        }
    }
}
