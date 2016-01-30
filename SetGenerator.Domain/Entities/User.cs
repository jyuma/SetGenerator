using System;
using System.Collections.Generic;

namespace SetGenerator.Domain.Entities
{
    public class User : EntityBase
    {
        public virtual string UserName { get; set; }
        public virtual string PasswordHash { get; set; }
        public virtual string Email { get; set; }
        public virtual DateTime DateRegistered { get; set; }
        public virtual string IPAddress { get; set; }
        public virtual string BrowserInfo { get; set; }
        public virtual bool IsDisabled { get; set; }
        public virtual string SecurityStamp { get; set; }
        public virtual string Descriminator { get; set; }
        public virtual bool EmailConfirmed { get; set; }
        public virtual bool LockoutEnabled { get; set; }
        public virtual DateTime? LockoutEndDateUtc { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual bool PhoneNumberConfirmed { get; set; }
        public virtual bool TwoFactorEnabled { get; set; }
        public virtual int AccessFailedCount { get; set; }
        public virtual IEnumerable<UserBand> UserBands { get; set; }
        public virtual IEnumerable<UserPreferenceTableColumn> UserPreferenceTableColumns { get; set; }
        public virtual IEnumerable<UserPreferenceTableMember> UserPreferenceTableMembers { get; set; }
    }
}
