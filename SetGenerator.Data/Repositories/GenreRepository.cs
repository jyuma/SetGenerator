using System.Collections;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IGenreRepository : IRepositoryBase<Genre>
    {
        ArrayList GetArrayList();
    }

    public class GenreRepository : RepositoryBase<Genre>, IGenreRepository
    {
        public GenreRepository(ISession session)
            : base(session)
        {
        }

        public ArrayList GetArrayList()
        {
            var genres = GetAll();

            var al = new ArrayList();

            foreach (var g in genres)
                al.Add(new { Value = g.Id, Display = g.Name });

            return al;
        }
    }
}
