using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IGigRepository : IRepositoryBase<Gig>
    {
    }

    public class GigRepository : RepositoryBase<Gig>, IGigRepository
    {
        public GigRepository(ISession session)
            : base(session)
        {
        }
    }
}
