namespace SetGenerator.Domain.Entities
{
    public class TableColumn : EntityBase
    {
        public virtual Table Table { get; set; }
        public virtual string Name { get; set; }
        public virtual string Data { get; set; }
        public virtual bool AlwaysVisible { get; set; }
    }
}
