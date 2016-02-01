using System;

namespace SetGenerator.Domain.Entities
{
    public class Gig : EntityAuditBase
    {
        public virtual Band Band { get; set; }
        public virtual DateTime DateGig { get; set; }
        public virtual string Venue { get; set; }
        public virtual string Description { get; set; }
        public virtual Setlist Setlist { get; set; }
    }
}
