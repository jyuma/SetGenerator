namespace SetGenerator.Domain.Entities
{
    public class UserPreferenceTableMember : EntityBase
    {
        public virtual User User { get; set; }
        public virtual Member Member { get; set; }
        public virtual Table Table { get; set; }
        public virtual bool IsVisible { get; set; }
    }
}
