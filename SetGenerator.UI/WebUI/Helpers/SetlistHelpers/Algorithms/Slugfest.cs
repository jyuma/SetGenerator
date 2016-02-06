using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SetGenerator.Domain.Entities;
using SetGenerator.Service;
using SetGenerator.WebUI.Extensions;

namespace SetGenerator.WebUI.Helpers.SetlistHelpers.Algorithms
{
    public static class Slugfest
    {
        private static IEnumerable<Song> _masterSongList;
        private static ICollection<SetSong> _setSongs;
        private static IEnumerable<SongMemberInstrument> _songMemberInstrumentList;

        private static class Container
        {
            public static int NumSets { get; set; }
            public static int NumSongsPerSet { get; set; }
            public static int SongSequence { get; set; }
            public static int CurrentSetNumber { get; set; }
            public static int DefaultSingerId { get; set; }

            public static Song LastSong { get; set; }

            public static bool IsNewSet { get; set; }
            public static bool IsLastSong { get; set; }

            public static int LastKeyId { get; set; }

            public static int SingerCount { get; set; }
            public static int LastSingerId { get; set; }

            public static int IntrumentCount { get; set; }
            public static bool IsSameInstrumentation { get; set; }
            public static IList<SongMemberInstrument> LastInstruments { get; set; }

            public static Dictionary<int, Song> DicSongs { get; set; }
        }

        public static IEnumerable<SetSong> Generate(int numSets, int numSongsPerSet, IEnumerable<Song> songs)
        {
            _masterSongList = songs.ToArray();
            _songMemberInstrumentList = _masterSongList.SelectMany(x => x.SongMemberInstruments).ToList();

            Container.NumSets = numSets;
            Container.NumSongsPerSet = numSongsPerSet;
            Container.DefaultSingerId = _masterSongList.First().Band.DefaultSinger.Id;
            Container.IsNewSet = false;
            Container.LastSong = null;
            Container.LastKeyId = 0;
            Container.SingerCount = 0;
            Container.LastSingerId = 0;
            Container.IntrumentCount = 0;
            Container.LastInstruments = null;
            Container.DicSongs = _masterSongList.ToDictionary(x => x.Id);
            _setSongs = new Collection<SetSong>();

            for (Container.CurrentSetNumber = 1; Container.CurrentSetNumber <= Container.NumSets; Container.CurrentSetNumber++)
            {
                Container.IsSameInstrumentation = true;
                Container.IntrumentCount = 0;
                // for each set song
                for (Container.SongSequence = 1; Container.SongSequence <= Container.NumSongsPerSet; Container.SongSequence++)
                {
                    var lastOrDefault = _setSongs.LastOrDefault();

                    if (lastOrDefault != null)
                    {
                        Container.LastSong = lastOrDefault.Song;
                        Container.LastSingerId = lastOrDefault.Song.Singer != null ? lastOrDefault.Song.Singer.Id : 0;
                        Container.LastInstruments = lastOrDefault.Song.SongMemberInstruments.ToList();
                    }

                    var nextSong = GetNextSong();

                    if (nextSong.Singer != null)
                    {
                        if (Container.LastSingerId == nextSong.Singer.Id)
                            Container.SingerCount++;
                        else
                            Container.SingerCount = 1;
                    }

                    if (Container.IsSameInstrumentation)
                        Container.IntrumentCount++;
                    else
                        Container.IntrumentCount = 1;

                    _setSongs.Add(new SetSong
                    {
                        SetNumber = Container.CurrentSetNumber,
                        Sequence = Container.SongSequence,
                        Song = nextSong
                    });
                }
            }

            return _setSongs;
        }

        private static Song GetNextSong()
        {
            Container.IsNewSet = (Container.SongSequence == 1);
            Container.IsLastSong = (Container.SongSequence == Container.NumSongsPerSet);
            Song nextSong = null;

            //------ FIRST SONG -------
            if (Container.IsNewSet)
            {
                nextSong = GetOpener();
            }

            //------ MIDDLE SONG -------
            else if (!Container.IsNewSet && !Container.IsLastSong)
            {
                nextSong = GetMiddle();
            }

            //------ LAST SONG -------
            else if (Container.IsLastSong)
            {
                nextSong = GetCloser();
            }

            return nextSong;
        }

        private static Dictionary<int, Song> GetEligibleUnusedSongs()
        {
            IEnumerable<Song> eligibleSongs = new Collection<Song>();
                
            if (Container.IsNewSet)
            {
                eligibleSongs = _masterSongList
                    .Where(x => x.NeverOpen == false);
            }
            else if (!Container.IsNewSet && !Container.IsLastSong)
            {
                eligibleSongs = _masterSongList;
            }
            else if (Container.IsLastSong)
            {
                eligibleSongs = _masterSongList.Where(x => x.NeverClose == false).ToArray();
            }

            var usedSongs = _setSongs != null
                ? _setSongs.Select(x => x.Song.Id)
                : new Collection<int>();

            var result = eligibleSongs
                .Where(x => !usedSongs.Contains(x.Id))
                .ToDictionary(x => x.Id);

            if (result.Count == 0)
            {
                result = _masterSongList
                    .Where(x => !usedSongs.Contains(x.Id))
                    .ToDictionary(x => x.Id, x => x);
            }

            return result;
        }

        private static Song GetOpener()
        {
            var rnd = new Random();
            var eligibleSongs = GetEligibleUnusedSongs();
            var eligibleIds = eligibleSongs.Keys.ToArray();
            var maxIds = eligibleSongs.ToList().Count;

            int idx;
            var numAttempts = 0;
            
            while (true)
            {
                numAttempts++;
                idx = rnd.Next(0, maxIds);

                var song = eligibleSongs[eligibleIds[idx]];
                var matchingSongs = song.GetMatchingSongs(_songMemberInstrumentList);

                if (matchingSongs.Count() >= Constants.MinSongInstrumentationCount)
                {
                    break;
                }

                if (numAttempts >= 100) break;
            }

            return eligibleSongs[eligibleIds[idx]];
        }

        private static Song GetCloser()
        {
            var eligibleSongs = GetEligibleUnusedSongs();
            var matchingSongs = Container.LastSong.GetMatchingSongs(_songMemberInstrumentList);
            var dicPreviousMatchingSongs = matchingSongs
                .Where(x => x.IsDisabled == false)
                .ToDictionary(x => x.Id, x => x);

            // try to find the same instrumentation as the last song
            var closingEligibleSongs = eligibleSongs.Where(x => dicPreviousMatchingSongs
                .ContainsKey(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);

            if (!(closingEligibleSongs.Count > 0))
            {
                closingEligibleSongs = eligibleSongs;
            }

            return ApplyRules(closingEligibleSongs);
        }

        private static Song GetMiddle()
        {
            var eligibleSongs = GetEligibleUnusedSongs();
            IDictionary<int, Song> middleEligibleSongs;
            var matchingSongs = Container.LastSong.GetMatchingSongs(_songMemberInstrumentList);

            var dicPreviousMatchingSongs = matchingSongs
                .Where(x => x.IsDisabled == false)
                .ToDictionary(x => x.Id, x => x);

            if (Container.IntrumentCount < Constants.MinSongInstrumentationCount)
            {
                Container.IsSameInstrumentation = true;

                // try to find the same instrumentation as the last song
                middleEligibleSongs = eligibleSongs.Where(x => dicPreviousMatchingSongs
                    .ContainsKey(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                Container.IsSameInstrumentation = false;

                // try to find different instrumentation than the last song
                var filteredEligibleKeys = eligibleSongs.Keys.Except(dicPreviousMatchingSongs.Keys);

                middleEligibleSongs = filteredEligibleKeys.Select(x => eligibleSongs[x]).ToDictionary(x => x.Id);
            }

            if (!(middleEligibleSongs.Count > 0))
            {
                middleEligibleSongs = eligibleSongs;
            }

            return ApplyRules(middleEligibleSongs);
        }

        private static Song ApplyRules(IDictionary<int, Song> eligibleSongs)
        {
            var rnd = new Random();
            var rank = 0;
            var numAttempts = 0;

            Song song;

            var eligibleIds = eligibleSongs.Keys.ToArray();
            var maxIds = eligibleSongs.Count;

            while (true)
            {
                numAttempts++;
                var idx = rnd.Next(0, maxIds);

                song = eligibleSongs[eligibleIds[idx]];
                
                // if a middle song and an instrument change try to find one that has a few other songs with the same instrumentation
                if ((!Container.IsLastSong) && (Container.IsSameInstrumentation == false))
                {
                    var matchingSongs = song.GetMatchingSongs(_songMemberInstrumentList);
                    rank = matchingSongs.Count() >= Constants.MinSongInstrumentationCount 
                        ? 1 : 2;
                }

                else if ((Container.LastSingerId == Container.DefaultSingerId) && (Container.SingerCount < Constants.MaxDefaultSingerCount))
                {
                    rank = 1;
                }

                else if ((Container.LastSingerId != Container.DefaultSingerId) && (Container.LastSingerId != (song.Singer != null ? song.Singer.Id : 0)))
                {
                    rank = 1;
                }
                else if (Container.IsSameInstrumentation && Container.IntrumentCount <= Constants.MinSongInstrumentationCount)
                {
                    rank = 2;
                }
                else if (Container.IsSameInstrumentation && Container.IntrumentCount > Constants.MinSongInstrumentationCount)
                {
                    rank = 2;
                }
                else if ((Container.LastSingerId == Container.DefaultSingerId) && (Container.SingerCount < Constants.MaxDefaultSingerCount))
                {
                    rank = 3;
                }

                else if ((Container.LastSingerId != Container.DefaultSingerId) && (Container.LastSingerId != (song.Singer != null ? song.Singer.Id : 0)))
                {
                    rank = 4;
                }

                // tolerance
                if (rank == 1)
                {
                    break;
                }
                if ((rank == 2) && numAttempts > 500)
                {
                    break;
                }
                if ((rank == 3) && numAttempts > 1000)
                {
                    break;
                }
                if ((rank == 4) && numAttempts > 1500)
                {
                    break;
                }
                if (numAttempts > 2000)
                {
                    break;
                }
            }

            return song;
        }
    }
}