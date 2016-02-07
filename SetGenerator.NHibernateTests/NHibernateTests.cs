using System.Collections.Generic;
using System.Linq;
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
            const int MemberId_Blair = 3;

            using (var db = TestSessionFactory.OpenSession())
            {
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
            const int UserId_John = 10020;

            using (var db = TestSessionFactory.OpenSession())
            {
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
            const int UserId_John = 10020;
            const int BandId_Stetson = 3;
            const int TableId_Setlists = 6;

            using (var db = TestSessionFactory.OpenSession())
            {
                var columns = db.QueryOver<UserPreferenceTableColumn>()
                    .Where(x => x.User.Id == UserId_John)
                    .Where(x => x.Band.Id == BandId_Stetson)
                      .JoinQueryOver(uptc => uptc.TableColumn)
                      .Where(x => x.Table.Id == TableId_Setlists)
                      .List()
                      .OrderBy(o => o.TableColumn.Sequence);

                Assert.AreEqual(columns.Count(), 4);
            }
        }

        [TestMethod]
        public void GetUserTableMembers()
        {
            const int UserId_John = 10020;

            using (var db = TestSessionFactory.OpenSession())
            {
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
            const int BandId_Slugfest = 1;

            using (var db = TestSessionFactory.OpenSession())
            {
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
            const int SetlistId = 1;

            using (var db = TestSessionFactory.OpenSession())
            {
                var setlist = db.QueryOver<Setlist>().Where(x => x.Id == SetlistId)
                    .List()
                    .FirstOrDefault();

            }
        }

        [TestMethod]
        public void GetSongMemberInstrumentationMatches()
        {
            const int BandId_Slugfest = 1;
            const int SongId_JumboArk = 2;

            using (var db = TestSessionFactory.OpenSession())
            {
                IEnumerable<SongMemberInstrument> allBandSongMemberInstruments = 
                    db.QueryOver<Song>().Where(x => x.Band.Id == BandId_Slugfest).List()
                    .SelectMany(x => x.SongMemberInstruments).ToList();

                var result = allBandSongMemberInstruments
                        .Join(allBandSongMemberInstruments,
                            smi1 => new { smi1.Member, smi1.Instrument },
                            smi2 => new { smi2.Member, smi2.Instrument },
                            (smi2, smi1) => new { smi2, smi1 })
                            .Select(x => new
                            {
                                SongId = x.smi1.Song.Id,
                                MemberId = x.smi1.Member.Id,
                                MatchingSong = x.smi2.Song
                            }).Join(allBandSongMemberInstruments
                            .GroupBy(g => g.Song)
                            .Select(g => new
                            {
                                SongId = g.Key.Id,
                                MemberCount = g.Count(c => c.Member != null)
                            }), smi1 => smi1.SongId, smi2 => smi2.SongId,
                                (smi1, smi2) => new { smi2.SongId, smi1.MemberId, smi1.MatchingSong, smi2.MemberCount })
                        .Where(x => x.SongId != x.MatchingSong.Id).GroupBy(g => new { g.SongId, g.MatchingSong, g.MemberCount })
                        .Select(g => new
                        {
                            g.Key.SongId,
                            g.Key.MemberCount,
                            g.Key.MatchingSong,
                            MatchingMemberCount = g.Count(c => c.MemberId > 0)
                        })
                        .Where(x => x.MemberCount == x.MatchingMemberCount)
                        .Where(x => x.SongId == SongId_JumboArk)
                        .Select(x => x.MatchingSong)
                        .ToArray();

                Assert.IsTrue(result.Count() == 7);
            }
        }

        #region test mess
        //var songMemberInstruments2 = songMemberInstruments1;
        //var songMemberInstruments3 = songMemberInstruments1;
        //IEnumerable<Song> songs = db.QueryOver<Song>().Where(x => x.Band.Id == 1).List();

        //var list = songMemberInstruments1
        //    .Join(songMemberInstruments2
        //    .Join(songs.Where(x => x.Id == SongId_JumboArk), smi => smi.Song.Id, s => s.Id, (smi, s) => smi),
        //        smi1 => new {smi1.Member, smi1.Instrument},
        //        smi2 => new {smi2.Member, smi2.Instrument},
        //        (smi2, smi1) => new { smi2, smi1 })
        //        .Select(x => new
        //        {
        //            SongId = x.smi1.Song.Id,
        //            MemberId = x.smi1.Member.Id,
        //            MatchingSongId =  x.smi2.Song.Id,
        //        })
        //        .ToArray();

        //var list2 = songMemberInstruments3
        //    .Join(songs, smi => smi.Song.Id, s => s.Id, (smi, s) => smi)
        //    .GroupBy(g => g.Song)
        //    .Select(g => new
        //    {
        //        SongId = g.Key.Id,
        //        MemberCount = g.Count(c => c.Member != null)
        //    }).ToArray();

        //var list3 = songMemberInstruments1
        //    .Join(songMemberInstruments2
        //    .Join(songs.Where(x => x.Id == SongId_JumboArk), smi => smi.Song.Id, s => s.Id, (smi, s) => smi),
        //        smi1 => new { smi1.Member, smi1.Instrument },
        //        smi2 => new { smi2.Member, smi2.Instrument },
        //        (smi2, smi1) => new { smi2, smi1 })
        //        .Select(x => new
        //        {
        //            SongId = x.smi1.Song.Id,
        //            MemberId = x.smi1.Member.Id,
        //            MatchingSongId = x.smi2.Song.Id,
        //        }).Join(songMemberInstruments3
        //        .Join(songs, smi => smi.Song.Id, s => s.Id, (smi, s) => smi)
        //        .GroupBy(g => g.Song)
        //        .Select(g => new
        //        {
        //            SongId = g.Key.Id,
        //            MemberCount = g.Count(c => c.Member != null)
        //        }), smi1 => smi1.SongId, smi2 => smi2.SongId,
        //    (smi1, smi2) => new { smi2.SongId, smi1.MemberId, smi1.MatchingSongId, smi2.MemberCount })
        //    .Where(x => x.SongId != x.MatchingSongId).ToArray();
        #endregion
    }
}
