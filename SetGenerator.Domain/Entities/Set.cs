using System.Collections.Generic;

namespace SetGenerator.Domain.Entities
{
    public class Set : EntityBase
    {
        public virtual Setlist Setlist { get; set; }
        public virtual int Number{ get; set; }
        public virtual Song Song{ get; set; }
        public virtual int Sequence{ get; set; }
        public virtual IEnumerable<Song> Songs { get; set; }
    }
}
