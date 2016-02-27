using System.Collections;
using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IGenreRepository : IRepositoryBase<Genre>
    {
        Genre GetByName(string name);
        ArrayList GetNameArrayList();
    }

    public class GenreRepository : RepositoryBase<Genre>, IGenreRepository
    {
        public GenreRepository(ISession session)
            : base(session)
        {
        }

        public Genre GetByName(string name)
        {
            return Session.QueryOver<Genre>()
                .Where(x => x.Name == name)
                .SingleOrDefault();
        }

        public ArrayList GetNameArrayList()
        {
            var instruments = GetAll().OrderBy(o => o.Name);

            var al = new ArrayList();

            foreach (var i in instruments)
                al.Add(new { Value = i.Id, Display = i.Name });

            return al;
        }
    }
}