namespace SetGenerator.Domain.Entities
{
    public class SongMemberInstrument : EntityBase
    {
        public virtual Band Band { get; set; }
        public virtual Song Song { get; set; }
        public virtual Member Member { get; set; }
        public virtual Instrument Instrument { get; set; }
    }
}
