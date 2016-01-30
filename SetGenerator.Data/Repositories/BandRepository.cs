using System.Collections;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IBandRepository : IRepositoryBase<Band>
    {
        ArrayList GetNameArrayList();
    }

    public class BandRepository : RepositoryBase<Band>, IBandRepository
    {
        public BandRepository(ISession session)
            : base(session)
        {
        }

        public ArrayList GetNameArrayList()
        {
            var bands = GetAll();
            if (bands == null) return null;
            var al = new ArrayList();

            foreach (var b in bands)
                al.Add(new { Value = b.Id, Display = b.Name });

            return al;
        }
    }
}
