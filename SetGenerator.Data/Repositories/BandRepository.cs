using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IBandRepository : IRepositoryBase<Band>
    {
        ArrayList GetNameArrayList();

        // Member
        IEnumerable<Member> GetMembers(int bandId);
        IEnumerable<string> GetMemberNameList(int bandId);
        ArrayList GetMemberNameArrayList(int bandId);
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


        // Member

        public IEnumerable<Member> GetMembers(int bandId)
        {
            var list = Session.QueryOver<Member>()
                .Where(x => x.Band.Id == bandId)
                .List()
                .OrderBy(x => x.FirstName);

            return list;
        }

        public IEnumerable<string> GetMemberNameList(int bandId)
        {
            var list = Session.QueryOver<Member>()
                .Where(x => x.Band.Id == bandId)
                .List()
                .Distinct()
                .OrderBy(x => x.FirstName)
                .Select(x => x.FirstName);

            return list;
        }

        public ArrayList GetMemberNameArrayList(int bandId)
        {
            var members = Session.QueryOver<Member>()
                .Where(x => x.Band.Id == bandId)
                .List()
                .ToArray();

            var al = new ArrayList();

            foreach (var m in members)
                al.Add(new { Value = m.Id, Display = m.FirstName });

            return al;
        }
    }
}
