using System.Collections.Generic;

namespace SetGenerator.Domain.Entities
{
    public class Setlist : EntityAuditBase
    {
        public virtual Band Band { get; set; }
        public virtual string Name { get; set; }
        public virtual User UserUpdate { get; set; }
        public virtual User UserCreate { get; set; }
        public virtual IEnumerable<Set> Sets { get; set; }
    }
}
