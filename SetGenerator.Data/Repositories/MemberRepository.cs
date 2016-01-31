using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IMemberRepository : IRepositoryBase<Member>
    {
        IEnumerable<Member> GetByBandId(int bandId);
        IEnumerable<string> GetNameList(int bandId);
        ArrayList GetNameArrayList(int bandId);
    }

    public class MemberRepository : RepositoryBase<Member>, IMemberRepository
    {
        public MemberRepository(ISession session)
            : base(session)
        {
        }

        public IEnumerable<Member> GetByBandId(int bandId)
        {
            var list = Session.QueryOver<Member>()
                .Where(x => x.Band.Id == bandId)
                .List()
                .OrderBy(x => x.FirstName);

            return list;
        }

        public IEnumerable<string> GetNameList(int bandId)
        {
            var list = Session.QueryOver<Member>()
                .Where(x => x.Band.Id == bandId)
                .List()
                .Distinct()
                .OrderBy(x => x.FirstName)
                .Select(x => x.FirstName);

            return list;
        }

        public ArrayList GetNameArrayList(int bandId)
        {
            var members = GetAll()
                .Where(x => x.Band.Id == bandId);

            var al = new ArrayList();

            foreach (var m in members)
                al.Add(new { Value = m.Id, Display = m.FirstName });

            return al;
        }
    }
}
