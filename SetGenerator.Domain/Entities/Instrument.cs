namespace SetGenerator.Domain.Entities
{
    public class Instrument : EntityBase
    {
        public virtual string Name { get; set; }
        public virtual string Abbreviation { get; set; }
    }
}
