using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain;

namespace SetGenerator.Repositories
{
    public interface IGigRepository
    {
        // database routines
        int Add(Gig g);
        void Delete(Gig g);
        void Update();

        Gig GetSingle(int id);
        IEnumerable<Gig> GetList(int bandId);
    }
    
    public class GigRepository : IGigRepository
    {
        private readonly SetGeneratorEntities _context;

        public GigRepository(SetGeneratorEntities context)
        {
            _context = context;
        }

        public int Add(Gig g)
        {
            _context.Gigs.Add(g);
            _context.SaveChanges();
            return _context.Instruments.Max(x => x.Id);
        }

        public void Update()
        {
            _context.SaveChanges();
        }

        public void Delete(Gig g)
        {
            _context.Gigs.Remove(g);
            _context.SaveChanges();
        }

        public Gig GetSingle(int id)
        {
            var g = _context.Gigs
                .Include("User1")
                .FirstOrDefault(x => x.Id == id);
            return g;
        }

        public IEnumerable<Gig> GetList(int bandId)
        {
            var list = _context.Gigs
                .Include("User1")
                .Where(x => x.BandId == bandId)
                .OrderBy(x => x.DateUpdate)
                .ToList();
            return list;
        }
    }
}
