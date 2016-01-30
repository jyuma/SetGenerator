namespace SetGenerator.Domain.Entities
{
    public class MemberInstrument : EntityBase
    {
        public virtual Band Band { get; set; }
        public virtual Member Member { get; set; }
        public virtual Instrument Instrument { get; set; }
    }
}
