using SetGenerator.Domain;
using SetGenerator.Service;
using SetGenerator.WebUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;

namespace SetGenerator.WebUI.Extensions
{
    public static class SongExtensions
    {
        public static Setlist GenerateSets(this IList<Song> songs, SetlistDetail setlistDetail, int bandId, long userId)
        {
            var sets = new List<Set>();
            Setlist setlist = null;

            if (songs != null)
            {
                var defaultsingerid = songs[0].Band.DefaultSinger.Id;
                setlist = new Setlist
                {
                    //BandId = bandId,
                    Name = setlistDetail.Name,
                    DateCreate = DateTime.Now,
                    DateUpdate = DateTime.Now,
                    //UserCreate.Id = userId,
                    //UserUpdate.Id = userId
                };

                var numsets = setlistDetail.NumSets;
                var numsongs = setlistDetail.NumSongs;

                // for each set
                for (var setnum = 1; setnum <= numsets; setnum++)
                {
                    var seq = 0;
                    var singercount = 0;
                    var instrcount = 0;

                    // for each song per set
                    for (var song = 1; song <= numsongs; song++)
                    {
                        seq++;
                        var lastOrDefault = sets.LastOrDefault();

                        var lastsingerid = 0;
                        IList<SongMemberInstrument> lastinstruments = null;
                        if (lastOrDefault != null)
                        {
                            lastsingerid = lastOrDefault.Song.Singer.Id > 0 ? lastOrDefault.Song.Singer.Id : 0;
                            lastinstruments = lastOrDefault.Song.SongMemberInstruments.ToList();
                        }
                        var nextsong = GetNextSong(songs, setlist.Sets, defaultsingerid, (seq == numsongs), seq, singercount, lastsingerid, instrcount, lastinstruments);
                        if (lastsingerid == nextsong.Singer.Id)
                            singercount++;
                        else
                            singercount = 1;

                        var isSameInstrumentation = IsSameInstrumentation(lastinstruments, nextsong.SongMemberInstruments);
                        if (isSameInstrumentation)
                            instrcount++;
                        else
                            instrcount = 1;

                        sets.Add(new Set
                        {
                            Number = setnum,
                            Sequence = seq,
                            Song = nextsong
                        });
                        //setlist.Sets.Add(new Set
                        //{
                        //    Number = setnum,
                        //    Sequence = seq,
                        //    SongId = nextsong.Id,
                        //    UserCreateId = userId,
                        //    UserUpdateId = userId
                        //});
                    }
                }
            }
            return setlist;
        }

        private static bool IsSameInstrumentation(IEnumerable<SongMemberInstrument> lastInstrumentation, IEnumerable<SongMemberInstrument> newInstrumentation)
        {
            var returnval = true;
            if (lastInstrumentation == null) return false;
            var dicNew = newInstrumentation.ToDictionary(x => x.Member.Id, x => x.Instrument.Id);
            foreach (var ilast in lastInstrumentation.Where(ilast => ilast.Instrument.Id != dicNew[ilast.Member.Id]))
            {
                returnval = false;
            }

            return returnval;
        }

        private static Song GetNextSong(ICollection<Song> songs, IEnumerable<Set> setsongs, int defaultsingerid, bool islastsong, int seq, int singercount, int lastsingerid, int instrcount, IEnumerable<SongMemberInstrument> lastinstruments)
        {
            var rnd = new Random();

            var idx = 0;
            var isNewSet = (seq == 1);
            var dicSongs = songs.ToDictionary(x => x.Id);
            var validIds = GetValidIds(songs, setsongs, instrcount, lastinstruments);
            var maxids = validIds.GetUpperBound(0) + 1;

            //------ FIRST SONG -------
            if (isNewSet)
            {
                var numAttempts = 0;
                while (true)
                {
                    numAttempts++;
                    idx = rnd.Next(0, maxids);
                    if (dicSongs[validIds[idx]].NeverOpen == false)
                        break;
                    if (numAttempts == 1000)
                        break;
                }
            }

            //------ MIDDLE SONG -------
            if (!(isNewSet || islastsong))
            {
                var numAttempts = 0;
                while (true)
                {
                    numAttempts++;
                    idx = rnd.Next(0, maxids);
                    var s = dicSongs[validIds[idx]];
                    if (lastsingerid == defaultsingerid)
                    {
                        if (singercount < Constants.MaxDefaultSingerCount)
                            break;
                    }

                    if (lastsingerid != s.Singer.Id)
                    {
                        break;
                    }

                    if (numAttempts == 1000)
                        break;
                }
            }

            //------ LAST SONG -------
            if (islastsong)
            {
                var numAttempts = 0;
                while (true)
                {
                    numAttempts++;
                    idx = rnd.Next(0, maxids);
                    if (dicSongs[validIds[idx]].NeverClose == false)
                        break;
                    if (numAttempts == 1000)
                        break;
                }
            }

            return dicSongs[validIds[idx]];
        }

        private static int[] GetValidIds(ICollection<Song> songs, IEnumerable<Set> setsongs, int instrcount, IEnumerable<SongMemberInstrument> lastinstruments)
        {
            var eligibleSongs = new List<Song>();
            var usedSongs = setsongs.Select(x => x.Song.Id);

            if (lastinstruments != null)
            {
                var dicInstruments = lastinstruments.ToDictionary(x => x.Member.Id, x => x.Instrument.Id);
                if (instrcount < Constants.MinSongInstrumentationCount)
                {
                    foreach (var s in songs)
                    {
                        var isadd = true;
                        foreach (
                            var smi in
                                s.SongMemberInstruments.Where(smi => smi.Instrument.Id != dicInstruments[smi.Member.Id]))
                        {
                            isadd = false;
                        }
                        if (isadd)
                        {
                            eligibleSongs.Add(s);
                        }
                    }
                }
                else
                {
                    eligibleSongs.AddRange(songs);
                }
            }
            else
            {
                eligibleSongs.AddRange(songs);
            }
            var validIds = eligibleSongs
                .Where(x => !usedSongs.Contains(x.Id))
                .Select(x => x.Id)
                .ToArray();

            if (validIds.Count() == 0)
                validIds = songs
                   .Where(x => !usedSongs.Contains(x.Id))
                   .Select(x => x.Id)
                   .ToArray();

            return validIds;
        }

    }
}