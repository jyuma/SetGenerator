using System.Collections.Generic;

namespace SetGenerator.Domain.Entities
{
    public class Band : EntityAuditBase
    {
        public virtual string Name { get; set; }
        public virtual Member DefaultSinger { get; set; }
        public virtual IEnumerable<Member> Members { get; set; }
        public virtual IEnumerable<Song> Songs { get; set; }
        public virtual IEnumerable<Gig> Gigs { get; set; }
    }
}
