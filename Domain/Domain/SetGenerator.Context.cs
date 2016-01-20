﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SetGenerator.Domain
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class SetGeneratorEntities : DbContext
    {
        public SetGeneratorEntities()
            : base("name=SetGeneratorEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Band> Bands { get; set; }
        public virtual DbSet<Gig> Gigs { get; set; }
        public virtual DbSet<Instrument> Instruments { get; set; }
        public virtual DbSet<Key> Keys { get; set; }
        public virtual DbSet<KeyName> KeyNames { get; set; }
        public virtual DbSet<Member> Members { get; set; }
        public virtual DbSet<MemberInstrument> MemberInstruments { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Set> Sets { get; set; }
        public virtual DbSet<SetList> SetLists { get; set; }
        public virtual DbSet<Song> Songs { get; set; }
        public virtual DbSet<SongMemberInstrument> SongMemberInstruments { get; set; }
        public virtual DbSet<Table> Tables { get; set; }
        public virtual DbSet<TableColumn> TableColumns { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserBand> UserBands { get; set; }
        public virtual DbSet<UserClaim> UserClaims { get; set; }
        public virtual DbSet<UserLogin> UserLogins { get; set; }
        public virtual DbSet<UserPreferenceTableColumn> UserPreferenceTableColumns { get; set; }
        public virtual DbSet<UserPreferenceTableMember> UserPreferenceTableMembers { get; set; }
    }
}
