using SetGenerator.Domain.Entities;
using FluentNHibernate.Mapping;

namespace SetGenerator.Domain.Mappings
{
    public sealed class GigMap : ClassMap<Gig>
    {
        public GigMap()
        {
            Table("Gig");

            Id(x => x.Id).Column("Id");
            Map(m => m.DateGig).Column("DateGig");
            Map(m => m.Venue).Column("Venue");
            Map(m => m.Description).Column("Description");
            References(m => m.Setlist).Column("SetlistId").Nullable();
            References(m => m.Band).Column("BandId");
            References(m => m.UserCreate).Column("UserCreateId");
            References(m => m.UserUpdate).Column("UserUpdateId");
        }
    }
}
