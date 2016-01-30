namespace SetGenerator.Domain.Entities
{
    public class Key : EntityBase
    {
        public virtual KeyName KeyName { get; set; }
        public virtual int MajorMinor { get; set; }
        public virtual int SharpFlatNatural { get; set; }
    }
}
