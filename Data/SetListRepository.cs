using SetGenerator.Domain;
using System.Collections.Generic;
using System.Linq;

namespace SetGenerator.Repositories
{
    public interface ISetListRepository
    {
        // database routines
        int Add(SetList sl);
        void Delete(SetList sl);
        void Update();

        SetList GetSingle(int id);
        IEnumerable<SetList> GetList(int bandId);
        SetList GetByName(int bandId, string name);
    }

    public class SetListRepository : ISetListRepository
    {
        private readonly SetGeneratorEntities _context;

        public SetListRepository(SetGeneratorEntities context)
        {
            _context = context;
        }

        public int Add(SetList sl)
        {
            _context.SetLists.Add(sl);
            _context.SaveChanges();
            return sl.Id;
        }

        public void Update()
        {
            _context.SaveChanges();
        }

        public void Delete(SetList sl)
        {
            _context.SetLists.Remove(sl);
            _context.SaveChanges();
        }

        public SetList GetSingle(int id)
        {
            var b = _context.SetLists
                .Include("Sets.Song")
                .Include("User1")
                .Single(x => x.Id == id);
            return b;
        }

        public IEnumerable<SetList> GetList(int bandId)
        {
            var list = _context.SetLists
                .Include("Sets.Song")
                .Include("User1")
                .Where(x => x.BandId == bandId)
                .OrderByDescending(x => x.DateUpdate)
                .ToList();
            return list;
        }

        public SetList GetByName(int bandId, string name)
        {
            var b = _context.SetLists
                .Where(x => x.BandId == bandId)
                .FirstOrDefault(x => x.Name == name);
            return b;
        }
    }
}
