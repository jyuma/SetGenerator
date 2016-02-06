using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;

namespace SetGenerator.WebUI.Extensions
{
    public static class SongMemberInstrumentExtensions
    {
        public static IEnumerable<Song> GetMatchingSongs(this Song song, IEnumerable<SongMemberInstrument> 
            songMemberInstruments)
        {
            var allBandSongMemberInstruments = songMemberInstruments.ToArray();
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
                    .Where(x => x.SongId == song.Id)
                    .Select(x => x.MatchingSong)
                    .ToArray();

            return result;
        }
    }
}