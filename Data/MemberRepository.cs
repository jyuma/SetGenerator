using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain;

namespace SetGenerator.Repositories
{
    public interface IMemberRepository
    {
        // database routines
        int Add(Member m);
        void Delete(Member m);
        void Update();

        Member GetSingle(int id);
        IEnumerable<Member> GetList(int bandId);
        IEnumerable<string> GetNameList(int bandId);
        ArrayList GetNameArrayList(int bandId);
    }

    public class MemberRepository : IMemberRepository
    {

        private readonly SetGeneratorEntities _context;

        public MemberRepository(SetGeneratorEntities context)
        {
            _context = context;
        }

        public Member GetSingle(int id)
        {
            var s = _context.Members
                .Include("Instrument")
                .Include("MemberInstruments")
                .FirstOrDefault(x => x.Id == id);
            return s;
        }

        public IEnumerable<Member> GetList(int bandId)
        {
           var list = _context.Members
                .Include("Instrument")
                .Include("MemberInstruments")
                .Where(x => x.BandId == bandId)
                .OrderBy(x => x.Id)
                .ToList();
            return list;
        }

        public int Add(Member m)
        {
            _context.Members.Add(m);
            _context.SaveChanges();
            return _context.Members.Max(x => x.Id);
        }

        public void Update()
        {
            _context.SaveChanges();
        }

        public void Delete(Member m)
        {
            _context.Members.Remove(m);
            _context.SaveChanges();
        }

        public IEnumerable<string> GetNameList(int bandId)
        {
            var list = GetList(bandId).OrderBy(x => x.FirstName);

            return list.Where(x => x.BandId == bandId)
                .Select(item => item.FirstName)
                .Distinct()
                .ToList();
        }

        public ArrayList GetNameArrayList(int bandId)
        {
            var members = GetList(bandId);
            if (members == null) return null;
            var al = new ArrayList();

            foreach (var m in members)
                al.Add(new { Value = m.Id, Display = m.FirstName });

            return al;
        }
    }
}
