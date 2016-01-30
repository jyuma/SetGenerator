namespace SetGenerator.Domain.Entities
{
    public class UserBand : EntityBase
    {
        public virtual Band Band { get; set; }
        public virtual User User { get; set; }
    }
}