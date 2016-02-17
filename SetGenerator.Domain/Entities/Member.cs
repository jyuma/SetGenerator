using System.Collections.Generic;

namespace SetGenerator.Domain.Entities
{
    public class Member : EntityBase
    {
        public virtual Band Band { get; set; }
        public virtual string FirstName{ get; set; }
        public virtual string LastName { get; set; }
        public virtual string Initials { get; set; }
        public virtual string Alias { get; set; }
        public virtual Instrument DefaultInstrument { get; set; }
        public virtual IList<MemberInstrument> MemberInstruments { get; set; }
    }
}
