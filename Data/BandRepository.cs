using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain;

namespace SetGenerator.Repositories
{
    public interface IBandRepository
    {
        // database routines
        int Add(Band b);

        void Delete(Band b);

        void Update();

        Band GetSingle(int id);

        IEnumerable<Band> GetList();

        ArrayList GetNameArrayList();
    }

    public class BandRepository : IBandRepository
    {
        private readonly SetGeneratorEntities _context;

        public BandRepository(SetGeneratorEntities context)
        {
            _context = context;
        }

        public int Add(Band b)
        {
            _context.Bands.Add(b);
            _context.SaveChanges();
            return _context.Instruments.Max(x => x.Id);
        }

        public void Update()
        {
            _context.SaveChanges();
        }

        public void Delete(Band b)
        {
            _context.Bands.Remove(b);
            _context.SaveChanges();
        }

        public Band GetSingle(int id)
        {
            var b = _context.Bands
                .FirstOrDefault(x => x.Id == id);
            return b;
        }

        public IEnumerable<Band> GetList()
        {
            var list = _context.Bands
                .ToList();
            return list;
        }

        public ArrayList GetNameArrayList()
        {
            var bands = GetList();
            if (bands == null) return null;
            var al = new ArrayList();

            foreach (var b in bands)
                al.Add(new { Value = b.Id, Display = b.Name });

            return al;
        }
    }
}
