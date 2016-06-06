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
        int AddRemoveMemberInstruments(int memberId, int[] instrumentIds);
        void DeleteSongMemberInstruments(int memberId);
        void DeleteMemberInstruments(int memberId);
        void DeleteMemberSetSongs(int bandId, int memberId);
        void DeleteMemberSongs(int bandId, int memberId);
        void DeleteUserPreferenceTableMembers(int memberId);
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
            int id = 0;
            var member = Session.QueryOver<Member>().Where(x => x.Id == memberId).SingleOrDefault();
            var existingIds = member.MemberInstruments.Select(x => x.Instrument.Id).ToArray();

            var addIds = instrumentIds
                .Select(x => x)
                .Where(x => !existingIds.Contains(x))
                .ToArray();

            if (addIds.Any())
                id = AddMemberInstruments(member, addIds);
            
            var removeIds = existingIds
                .Select(x => x)
                .Where(x => !instrumentIds.Contains(x))
                .ToArray();

            if (!removeIds.Any()) return id;
            
            RemoveMemberInstruments(member, removeIds);

            var songMemberInstruments = Session.QueryOver<SongMemberInstrument>()
                .Where(x => x.Member.Id == memberId)
                .List()
                .Where(x => removeIds.Contains(x.Instrument.Id));

            foreach (var songMemberInstrument in songMemberInstruments)
            {
                var smi = songMemberInstrument;
                var song = Session.QueryOver<Song>().Where(x => x.Id == smi.Song.Id).SingleOrDefault();
                song.SongMemberInstruments.Remove(smi);

                using (var transaction = Session.BeginTransaction())
                {
                    Session.Delete(songMemberInstrument);
                    transaction.Commit();
                }
            }
            

            return id;
        }

        public void DeleteSongMemberInstruments(int memberId)
        {
            var songMemberInstruments = Session.QueryOver<SongMemberInstrument>()
                .Where(x => x.Member.Id == memberId)
                .List();

            foreach (var songMemberInstrument in songMemberInstruments)
            {
                using (var transaction = Session.BeginTransaction())
                {
                    Session.Delete(songMemberInstrument);
                    transaction.Commit();
                }
            }
        }

        public void DeleteMemberSetSongs(int bandId, int memberId)
        {
            Song songTableAlias = null;

            var bandSetSongs = Session.QueryOver<SetSong>()
                .JoinAlias(x => x.Song, () => songTableAlias)
                .Where(x => x.Song.Id == songTableAlias.Id)
                .Where(x => songTableAlias.Band.Id == bandId)
                .Where(x => songTableAlias.Singer != null)
                .List();

            foreach (var bandSetSong in bandSetSongs)
            {
                if (bandSetSong.Song.Singer.Id != memberId) continue;

                using (var transaction = Session.BeginTransaction())
                {
                    Session.Delete(bandSetSong);
                    transaction.Commit();
                }
            }
        }

        public void DeleteMemberSongs(int bandId, int memberId)
        {
            var bandSongs = Session.QueryOver<Song>()
              .Where(x => x.Band.Id == bandId)
              .Where(x => x.Singer != null)
              .List();

            foreach (var bandSong in bandSongs)
            {
                if (bandSong.Singer.Id != memberId) continue;

                using (var transaction = Session.BeginTransaction())
                {
                    Session.Delete(bandSong);
                    transaction.Commit();
                }
            }
        }

        public void DeleteMemberInstruments(int memberId)
        {
            var memberInstruments = Session.QueryOver<MemberInstrument>()
                .Where(x => x.Member.Id == memberId)
                .List();
           
            foreach (var memberInstrument in memberInstruments)
            {
                using (var transaction = Session.BeginTransaction())
                {
                    Session.Delete(memberInstrument);
                    transaction.Commit();
                }
            }
        }

        public void DeleteUserPreferenceTableMembers(int memberId)
        {
            var userPreferenceTableMembers = Session.QueryOver<UserPreferenceTableMember>()
                .Where(x => x.Member.Id == memberId)
                .List();

            foreach (var userPreferenceTableMember in userPreferenceTableMembers)
            {
                using (var transaction = Session.BeginTransaction())
                {
                    Session.Delete(userPreferenceTableMember);
                    transaction.Commit();
                }
            }
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
