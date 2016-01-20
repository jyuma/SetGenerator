using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain;

namespace SetGenerator.Repositories
{
    public interface IInstrumentRepository
    {
        // database routines
        int Add(Instrument i);
        void Delete(Instrument i);
        void Update();

        Instrument GetSingle(int id);
        IEnumerable<Instrument> GetList();
        ArrayList GetNameArrayList();
    }
    public class InstrumentRepository : IInstrumentRepository
    {
        private readonly SetGeneratorEntities _context;

        public InstrumentRepository(SetGeneratorEntities context)
        {
            _context = context;
        }

        public int Add(Instrument i)
        {
            _context.Instruments.Add(i);
            _context.SaveChanges();
            return _context.Instruments.Max(x => x.Id);
        }

        public void Update()
        {
            _context.SaveChanges();
        }

        public void Delete(Instrument i)
        {
            _context.Instruments.Remove(i);
            _context.SaveChanges();
        }


        public Instrument GetSingle(int id)
        {
            var i = _context.Instruments
                .FirstOrDefault(x => x.Id == id);
            return i;
        }

        public IEnumerable<Instrument> GetList()
        {
            var list = _context.Instruments
                 .OrderBy(x => x.Name)
                 .ToList();
            return list;
        }

        public ArrayList GetNameArrayList()
        {
            var instruments = GetList();
            if (instruments == null) return null;
            var al = new ArrayList();

            foreach (Instrument i in instruments)
                al.Add(new { Value = i.Id, Display = i.Name });

            return al;
        }
    }
}
