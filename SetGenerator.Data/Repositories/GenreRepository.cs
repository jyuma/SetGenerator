using System.Collections;
using System.Collections.Generic;
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

        public override ICollection<Genre> GetAll()
        {
            return Session.QueryOver<Genre>().List();
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