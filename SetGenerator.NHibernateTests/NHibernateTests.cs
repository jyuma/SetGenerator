using System.Linq;
using FluentNHibernate.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SetGenerator.Domain.Entities;

namespace SetGenerator.NHibernateTests
{
    [TestClass]
    public class NHibernateTests
    {
        [TestMethod]
        public void GetMemberInstruments()
        {
            using (var db = TestSessionFactory.OpenSession())
            {
                const int MemberId_Blair = 3;

                var member = db.QueryOver<Member>()
                    .Where(x => x.Id == MemberId_Blair)
                    .List()
                    .FirstOrDefault();

                var instruments = member.MemberInstruments.ToArray();

                Assert.AreEqual(instruments.Count(), 4);
            }
        }

        [TestMethod]
        public void GetUserBands()
        {
            using (var db = TestSessionFactory.OpenSession())
            {
                const int UserId_John = 10020;

                var user = db.QueryOver<User>()
                    .Where(x => x.Id == UserId_John)
                    .List()
                    .FirstOrDefault();

                var bands = user.UserBands.ToArray();

                Assert.AreEqual(bands.Count(), 3);
            }
        }

        [TestMethod]
        public void GetUserTableColumns()
        {
            using (var db = TestSessionFactory.OpenSession())
            {
                const int UserId_John = 10020;

                var user = db.QueryOver<User>()
                    .Where(x => x.Id == UserId_John)
                    .List()
                    .FirstOrDefault();

                var columns = user.UserPreferenceTableColumns.ToArray();

                Assert.AreEqual(columns.First().TableColumn.Name, "Title");
            }
        }

        [TestMethod]
        public void GetUserTableMembers()
        {
            using (var db = TestSessionFactory.OpenSession())
            {
                const int UserId_John = 10020;

                var user = db.QueryOver<User>().Where(x => x.Id == UserId_John)
                    .List()
                    .FirstOrDefault();

                var members = user.UserPreferenceTableMembers.ToArray();

                Assert.AreEqual(members.First().Table.Name, "Song");
            }
        }

        [TestMethod]
        public void GetBandSongs()
        {
            using (var db = TestSessionFactory.OpenSession())
            {
                const int BandId_Slugfest = 1;

                var band = db.QueryOver<Band>().Where(x => x.Id == BandId_Slugfest)
                    .List()
                    .FirstOrDefault();

                var songs = band.Songs.ToArray();

                Assert.AreEqual(songs.First().Title, "Chloe");
            }
        }

        [TestMethod]
        public void GetSetlistSongs()
        {
            using (var db = TestSessionFactory.OpenSession())
            {
                const int SetlistId = 1;

                var setlist = db.QueryOver<Setlist>().Where(x => x.Id == SetlistId)
                    .List()
                    .FirstOrDefault();

            }
        }

        [TestMethod]
        public void GetSongMemberInstrumentationMatches()
        {
            using (var db = TestSessionFactory.OpenSession())
            {
                const int SongId_Chloe = 1;

                var song = db.QueryOver<Song>().Where(x => x.Id == SongId_Chloe).SingleOrDefault();

                var matches = song.SongMemberInstrumentMatches;

                Assert.IsTrue(matches.Count == 0);
            }
        }
    }
}
