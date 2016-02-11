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
        IEnumerable<string> GetSingerNameList(int bandId);
        ArrayList GetMemberNameArrayList(int bandId);
        ArrayList GetSingerNameArrayList(int bandId);
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

        // Member / Singer

        public IEnumerable<Member> GetMembers(int bandId)
        {
            var list = Session.QueryOver<Member>()
                .Where(x => x.Band.Id == bandId)
                .List()
                .OrderBy(x => x.FirstName);

            return list;
        }

        public IEnumerable<string> GetSingerNameList(int bandId)
        {
            var singers = GetBandSingers(bandId);

            return singers.Select(x => x.Value);
        }

        public ArrayList GetMemberNameArrayList(int bandId)
        {
            var members = Session.QueryOver<Member>()
                .Where(x => x.Band.Id == bandId)
                .List()
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToArray();

            var al = new ArrayList();

            foreach (var m in members)
                al.Add(new { Value = m.Id, Display = m.FirstName });

            return al;
        }

        public ArrayList GetSingerNameArrayList(int bandId)
        {
            var singers = GetBandSingers(bandId);
            var al = new ArrayList();

            foreach (var s in singers)
                al.Add(new { Value = s.Key, Display = s.Value });

            return al;
        }

        private Dictionary<int, string> GetBandSingers(int bandId)
        {
            Member memberAlias = null;

            return Session.QueryOver<Song>()
                .Where(x => x.Band.Id == bandId)
                .JoinAlias(x => x.Singer, () => memberAlias)
                .Where(x => x.Singer.Id == memberAlias.Id)
                .List()
                .Select(x => new
                {
                    x.Singer.Id,
                    x.Singer.FirstName
                })
                .Distinct()
                .OrderBy(o => o.FirstName)
                .ToDictionary(x => x.Id, y => y.FirstName);
        }
    }
}
