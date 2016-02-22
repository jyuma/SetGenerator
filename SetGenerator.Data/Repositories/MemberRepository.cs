using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        int AddRemoveMemberInstruments(int memberId, int[] instrumentIds);
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

        public int AddRemoveMemberInstruments(int memberId, int[] instrumentIds)
        {
            var member = Session.QueryOver<Member>().Where(x => x.Id == memberId).SingleOrDefault();
            var existingIds = member.MemberInstruments.Select(x => x.Instrument.Id).ToArray();

            var addIds = instrumentIds
                .Select(x => x)
                .Where(x => !existingIds.Contains(x));

            var id = AddMemberInstruments(member, addIds);

            var removeIds = existingIds
                .Select(x => x)
                .Where(x => !instrumentIds.Contains(x));

            RemoveMemberInstruments(member, removeIds);

            return id;
        }

        private int AddMemberInstruments(Member member, IEnumerable<int> instrumentIds)
        {
            var result = 0;

            foreach (var memberInstrument in instrumentIds
                .Select(addIdLocal => Session.QueryOver<Instrument>()
                .Where(x => x.Id == addIdLocal)
                .SingleOrDefault())
                .Select(instrument => new MemberInstrument
                {
                    Instrument = instrument,
                    Member = member
                }))
            {
                using (var transaction = Session.BeginTransaction())
                {
                    result = (int)Session.Save(memberInstrument);
                    transaction.Commit();
                }
            }

            return result;
        }

        private void RemoveMemberInstruments(Member member, IEnumerable<int> instrumentIds)
        {
            foreach (var memberInstrument in instrumentIds
                .Select(removeIdLocal => member.MemberInstruments
                .SingleOrDefault(x => x.Instrument.Id == removeIdLocal)))
            {
                member.MemberInstruments.Remove(memberInstrument);
                using (var transaction = Session.BeginTransaction())
                {
                    Session.Delete(memberInstrument);
                    transaction.Commit();
                }
            }
        }
    }
}
