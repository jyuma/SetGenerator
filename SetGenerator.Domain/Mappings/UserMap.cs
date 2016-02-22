using FluentNHibernate.Mapping;
using SetGenerator.Domain.Entities;

namespace SetGenerator.Domain.Mappings
{
    public sealed class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Table("`User`");

            Id(x => x.Id).Column("Id");
            Map(m => m.UserName).Column("UserName");
            Map(m => m.Email).Column("Email");
            Map(m => m.PasswordHash).Column("PasswordHash");
            Map(m => m.DateRegistered).Column("DateRegistered");
            Map(m => m.IPAddress).Column("IPAddress");
            Map(m => m.BrowserInfo).Column("BrowserInfo");
            Map(m => m.IsDisabled).Column("IsDisabled");
            Map(m => m.SecurityStamp).Column("SecurityStamp");
            Map(m => m.Descriminator).Column("Descriminator");
            Map(m => m.EmailConfirmed).Column("EmailConfirmed");
            Map(m => m.LockoutEnabled).Column("LockoutEnabled");
            Map(m => m.LockoutEndDateUtc).Column("LockoutEndDateUtc");
            Map(m => m.PhoneNumber).Column("PhoneNumber");
            Map(m => m.PhoneNumberConfirmed).Column("PhoneNumberConfirmed");
            Map(m => m.TwoFactorEnabled).Column("TwoFactorEnabled");
            Map(m => m.AccessFailedCount).Column("AccessFailedCount");

            References(m => m.DefaultBand).Column("DefaultBandId").Nullable();

            HasMany(m => m.UserBands)
                .Cascade.All()
                .Inverse()
                .Cascade.DeleteOrphan();

            HasMany(m => m.UserPreferenceTableColumns)
                .Cascade.All()
                .Inverse()
                .Cascade.DeleteOrphan();

            HasMany(m => m.UserPreferenceTableMembers)
                .Cascade.All()
                .Inverse()
                .Cascade.DeleteOrphan();
        }
    }
}
