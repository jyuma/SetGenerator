//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class Gig
    {
        public int Id { get; set; }
        public int BandId { get; set; }
        public System.DateTime DateGig { get; set; }
        public string Venue { get; set; }
        public string Description { get; set; }
        public Nullable<int> SetListId { get; set; }
        public long UserCreateId { get; set; }
        public long UserUpdateId { get; set; }
        public System.DateTime DateCreate { get; set; }
        public System.DateTime DateUpdate { get; set; }
    
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
    }
}