using System.Collections;
using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IInstrumentRepository : IRepositoryBase<Instrument>
    {
        ArrayList GetNameArrayList();
    }

    public class InstrumentRepository : RepositoryBase<Instrument>, IInstrumentRepository
    {
        public InstrumentRepository(ISession session)
            : base(session)
        {
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