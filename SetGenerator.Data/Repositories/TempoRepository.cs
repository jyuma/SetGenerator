using System.Collections;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface ITempoRepository : IRepositoryBase<Tempo>
    {
        ArrayList GetArrayList();
    }

    public class TempoRepository : RepositoryBase<Tempo>, ITempoRepository
    {
        public TempoRepository(ISession session)
            : base(session)
        {
        }

        public ArrayList GetArrayList()
        {
            var tempos = GetAll();

            var al = new ArrayList();

            foreach (var t in tempos)
                al.Add(new { Value = t.Id, Display = t.Name });

            return al;
        }
    }
}
