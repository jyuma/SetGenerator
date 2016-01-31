namespace SetGenerator.Domain.Entities
{
    public class MemberInstrument : EntityBase
    {
        public virtual Member Member { get; set; }
        public virtual Instrument Instrument { get; set; }
    }
}
