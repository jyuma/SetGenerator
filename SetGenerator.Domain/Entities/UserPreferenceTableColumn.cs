namespace SetGenerator.Domain.Entities
{
    public class UserPreferenceTableColumn : EntityBase
    {
        public virtual User User { get; set; }
        public virtual Band Band { get; set; }
        public virtual TableColumn TableColumn { get; set; }
        public virtual bool IsVisible { get; set; }
    }
}
