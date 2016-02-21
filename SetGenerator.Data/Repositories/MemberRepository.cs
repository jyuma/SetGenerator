using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;
using NHibernate;

namespace SetGenerator.Data.Repositories
{
    public interface IMemberRepository : IRepositoryBase<Member>
    {
        Member GetByBandIdAlias(int bandId, string alias);
        IEnumerable<Member> GetByBandId(int bandId);
        IEnumerable<string> GetNameList(int bandId);
        ArrayList GetNameArrayList(int bandId);
        IEnumerable<Instrument> GetInstruments(int id);
    }

    public class MemberRepository : RepositoryBase<Member>, IMemberRepository
    {
        public MemberRepository(ISession session)
            : base(session)
        {
        }

        public Member GetByBandIdAlias(int bandId, string alias)
        {
            return Session.QueryOver<Member>()
                .Where(x => x.Alias == alias)
                .Where(x => x.Band.Id == bandId)
                .SingleOrDefault();
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

        public IEnumerable<Instrument> GetInstruments(int id)
        {
            return Session.QueryOver<MemberInstrument>()
                .Where(x => x.Member.Id == id)
                .List()
                .Select(x => x.Instrument)
                .OrderBy(x => x.Name);
        }
    }
}
