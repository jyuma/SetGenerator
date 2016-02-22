namespace SetGenerator.Domain.Entities
{
    public class UserBand : EntityBase
    {
        public virtual User User { get; set; }
        public virtual Band Band { get; set; }
    }
}