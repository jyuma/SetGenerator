using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface ITempoRepository : IRepositoryBase<Tempo>
    {
    }

    public class TempoRepository : RepositoryBase<Tempo>, ITempoRepository
    {
        public TempoRepository(ISession session)
            : base(session)
        {
        }
    }
}
