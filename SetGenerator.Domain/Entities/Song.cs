using System.Collections.Generic;

namespace SetGenerator.Domain.Entities
{
    public class Song : EntityAuditBase
    {
        public virtual Band Band { get; set; }
        public virtual string Title { get; set; }
        public virtual string Composer { get; set; }
        public virtual Member Singer{ get; set; }
        public virtual Key Key{ get; set; }
        public virtual bool NeverClose{ get; set; }
        public virtual bool NeverOpen { get; set; }
        public virtual Genre Genre { get; set; }
        public virtual Tempo Tempo { get; set; }
        public virtual bool IsDisabled { get; set; }
        public virtual IList<SongMemberInstrument> SongMemberInstruments { get; set; }
        public virtual User UserUpdate { get; set; }
        public virtual User UserCreate { get; set; }
    }
}
