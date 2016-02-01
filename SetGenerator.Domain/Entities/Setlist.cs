using System.Collections.Generic;

namespace SetGenerator.Domain.Entities
{
    public class Setlist : EntityAuditBase
    {
        public virtual Band Band { get; set; }
        public virtual string Name { get; set; }
        public virtual IList<SetSong> SetSongs { get; set; }
    }
}
