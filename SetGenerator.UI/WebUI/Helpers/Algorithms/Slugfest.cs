using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SetGenerator.Domain.Entities;
using SetGenerator.Service;

namespace SetGenerator.WebUI.Helpers.Algorithms
{
    public static class Slugfest
    {
        private static IEnumerable<Song> _masterSongList;
        private static ICollection<SetSong> _setSongs;

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
            var openingEligibleSongs = eligibleSongs
                .Where(x => x.Value.SongMemberInstrumentMatches.Count >= Constants.MinSongInstrumentationCount)
                .ToDictionary(x => x.Key, x => x.Value);

            if (openingEligibleSongs.Count == 0)
            {
                openingEligibleSongs = eligibleSongs;
            }

            int idx;
            var numAttempts = 0;
            
            while (true)
            {
                numAttempts++;
                idx = rnd.Next(0, maxIds);

                var song = openingEligibleSongs[eligibleIds[idx]];

                if (song.SongMemberInstrumentMatches.Count >= Constants.MinSongInstrumentationCount)
                {
                    break;
                }

                if (numAttempts >= 1000) break;
            }

            return eligibleSongs[eligibleIds[idx]];
        }

        private static Song GetCloser()
        {
            var eligibleSongs = GetEligibleUnusedSongs();

            return ApplyRules(eligibleSongs);
        }

        private static Song GetMiddle()
        {
            var eligibleSongs = GetEligibleUnusedSongs();
            IDictionary<int, Song> middleEligibleSongs;
            
            if (Container.IntrumentCount < Constants.MinSongInstrumentationCount)
            {
                Container.IsSameInstrumentation = true;
                middleEligibleSongs = Container.LastSong.SongMemberInstrumentMatches
                    .Select(x => x.MatchingSong)
                    .ToDictionary(x => x.Id, x => x);
            }
            else
            {
                Container.IsSameInstrumentation = false;
                var dicPreviousMatchingSongs = Container.LastSong.SongMemberInstrumentMatches
                    .ToDictionary(x => x.MatchingSong.Id, x => x);

                var newEligibleKeys = eligibleSongs.Keys.Except(dicPreviousMatchingSongs.Keys);

                middleEligibleSongs = newEligibleKeys.Select(x => eligibleSongs[x]).ToDictionary(x => x.Id);
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

                if ((Container.LastSingerId == Container.DefaultSingerId) && (Container.SingerCount < Constants.MaxDefaultSingerCount))
                {
                    rank = 1;
                }

                else if ((Container.LastSingerId != Container.DefaultSingerId) && (Container.LastSingerId != (song.Singer != null ? song.Singer.Id : 0)))
                {
                    rank = 1;
                }

                else if ((Container.LastSingerId == Container.DefaultSingerId) && (Container.SingerCount < Constants.MaxDefaultSingerCount))
                {
                    rank = 2;
                }

                else if ((Container.LastSingerId != Container.DefaultSingerId) && (Container.LastSingerId != (song.Singer != null ? song.Singer.Id : 0)))
                {
                    rank = 3;
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
                if (numAttempts > 1500)
                {
                    break;
                }
            }

            return song;
        }
    }
}