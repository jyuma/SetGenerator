using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IMemberRepository : IRepositoryBase<Member>
    {
        IEnumerable<string> GetNameList(int bandId);
        ArrayList GetNameArrayList(int bandId);
    }

    public class MemberRepository : RepositoryBase<Member>, IMemberRepository
    {
        public MemberRepository(ISession session)
            : base(session)
        {
        }
    
        public IEnumerable<string> GetNameList(int bandId)
        {
            var list = GetAll()
                .Where(x => x.Band.Id == bandId)
                .Distinct()
                .OrderBy(x => x.FirstName)
                .Select(x => x.FirstName)
                .ToArray();

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
