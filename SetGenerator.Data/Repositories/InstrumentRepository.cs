using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IInstrumentRepository : IRepositoryBase<Instrument>
    {
        Instrument GetByName(string name);
        ArrayList GetNameArrayList();
    }

    public class InstrumentRepository : RepositoryBase<Instrument>, IInstrumentRepository
    {
        public InstrumentRepository(ISession session)
            : base(session)
        {
        }

        public override ICollection<Instrument> GetAll()
        {
            return Session.QueryOver<Instrument>().List();
        }

        public Instrument GetByName(string name)
        {
            return Session.QueryOver<Instrument>()
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