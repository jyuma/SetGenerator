using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IGenreRepository : IRepositoryBase<Genre>
    {
    }

    public class GenreRepository : RepositoryBase<Genre>, IGenreRepository
    {
        public GenreRepository(ISession session)
            : base(session)
        {
        }
    }
}
