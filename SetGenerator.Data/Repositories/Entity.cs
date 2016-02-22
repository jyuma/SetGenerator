namespace SetGenerator.Data.Repositories
{
    public interface IEntity<T>
    {
        T Id { get; set; }
    }

    public abstract class BaseEntity { }
 
}
