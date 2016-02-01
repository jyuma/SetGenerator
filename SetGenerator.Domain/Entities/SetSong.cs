namespace SetGenerator.Domain.Entities
{
    public class SetSong : EntityBase
    {
        public virtual Setlist Setlist { get; set; }
        public virtual int SetNumber{ get; set; }
        public virtual Song Song{ get; set; }
        public virtual int Sequence{ get; set; }
    }
}
