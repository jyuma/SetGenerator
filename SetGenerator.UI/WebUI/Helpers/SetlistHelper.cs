using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SetGenerator.Data.Repositories;
using SetGenerator.Domain.Entities;
using SetGenerator.Service;
using SetGenerator.WebUI.ViewModels;

namespace SetGenerator.WebUI.Helpers
{
    public class SetlistHelper
    {
        private readonly IBandRepository _bandRepository;

        public SetlistHelper(IBandRepository bandRepository)
        {
            _bandRepository = bandRepository;
        }

        public Setlist GenerateSets(IEnumerable<Song> masterSongList, SetlistDetail setlistDetail, int bandId, User currentUser)
        {
            //var sets = new List<Set>();
            Setlist setlist = null;

            if (masterSongList != null)
            {
                var songs = masterSongList.ToArray();
                var defaultsingerid = songs.First().Band.DefaultSinger.Id;
                setlist = new Setlist
                {
                    Band = _bandRepository.Get(bandId),
                    Name = setlistDetail.Name,
                    DateCreate = DateTime.Now,
                    DateUpdate = DateTime.Now,
                    UserCreate = currentUser,
                    UserUpdate = currentUser,
                    SetSongs = new Collection<SetSong>()
                };

                var numsets = setlistDetail.NumSets;
                var numsongs = setlistDetail.NumSongs;

                // for each set
                for (var setnum = 1; setnum <= numsets; setnum++)
                {
                    var seq = 0;
                    var singercount = 0;
                    var instrcount = 0;

                    // for each set song
                    for (var song = 1; song <= numsongs; song++)
                    {
                        seq++;
                        var lastOrDefault = setlist.SetSongs.LastOrDefault();

                        var lastsingerid = 0;
                        IList<SongMemberInstrument> lastinstruments = null;

                        if (lastOrDefault != null)
                        {
                            lastsingerid = lastOrDefault.Song.Singer != null ? lastOrDefault.Song.Singer.Id : 0;
                            lastinstruments = lastOrDefault.Song.SongMemberInstruments.ToList();
                        }

                        var nextsong = GetNextSong(songs, setlist.SetSongs, defaultsingerid, (seq == numsongs), seq, singercount, lastsingerid, instrcount, lastinstruments);

                        if (nextsong.Singer != null)
                        {
                            if (lastsingerid == nextsong.Singer.Id)
                                singercount++;
                            else
                                singercount = 1;
                        }

                        var isSameInstrumentation = IsSameInstrumentation(lastinstruments, nextsong.SongMemberInstruments);
                        if (isSameInstrumentation)
                            instrcount++;
                        else
                            instrcount = 1;

                        setlist.SetSongs.Add(new SetSong
                        {
                            Setlist = setlist,
                            SetNumber = setnum,
                            Sequence = seq,
                            Song = nextsong
                        });
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

        private static Song GetNextSong(IEnumerable<Song> songs, IEnumerable<SetSong> setsongs, int defaultsingerid, bool islastsong, int seq, int singercount, int lastsingerid, int instrcount, IEnumerable<SongMemberInstrument> lastinstruments)
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

                    if (s.Singer != null)
                    {
                        if (lastsingerid != s.Singer.Id)
                        {
                            break;
                        }
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

        private static int[] GetValidIds(IEnumerable<Song> songs, IEnumerable<SetSong> setsongs, int instrcount, IEnumerable<SongMemberInstrument> lastinstruments)
        {
            var eligibleSongs = new List<Song>();
            var usedSongs = setsongs != null 
                ? setsongs.Select(x => x.Song.Id)
                : new Collection<int>();

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