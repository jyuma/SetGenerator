using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SetGenerator.Domain.Entities;
using SetGenerator.Service;

namespace SetGenerator.WebUI.Helpers.SetlistHelpers.Algorithms
{
    public static class StetsonBrothers
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

            public static bool IsNewSet { get; set; }
            public static bool IsLastSong { get; set; }

            public static int KeyCount { get; set; }
            public static int LastKeyId { get; set; }

            public static int TempoCount { get; set; }
            public static int LastTempoId { get; set; }

            public static int GenreCount { get; set; }
            public static int LastGenreId { get; set; }

            public static Dictionary<int, Song> DicSongs { get; set; }
        }

        public static IEnumerable<SetSong> Generate(int numSets, int numSongsPerSet, IEnumerable<Song> songs)
        {
            _masterSongList = songs.ToArray();
            
            Container.NumSets = numSets;
            Container.NumSongsPerSet = numSongsPerSet;
            Container.DefaultSingerId = _masterSongList.First().Band.DefaultSinger.Id;
            Container.IsNewSet = false;
            Container.KeyCount = 1;
            Container.LastKeyId = 0;
            Container.TempoCount = 1;
            Container.LastTempoId = 0;
            Container.GenreCount = 1;
            Container.LastGenreId = 0;
            Container.DicSongs = _masterSongList.ToDictionary(x => x.Id);

            _setSongs = new Collection<SetSong>();

            for (Container.CurrentSetNumber = 1; Container.CurrentSetNumber <= Container.NumSets; Container.CurrentSetNumber++)
            {
                // for each set song
                for (Container.SongSequence = 1; Container.SongSequence <= Container.NumSongsPerSet; Container.SongSequence++)
                {
                    var lastOrDefault = _setSongs.LastOrDefault();

                    if (lastOrDefault != null)
                    {
                        Container.LastKeyId = lastOrDefault.Song.Key.Id;
                        Container.LastTempoId = lastOrDefault.Song.Tempo.Id;
                        Container.LastGenreId = lastOrDefault.Song.Genre.Id;
                    }

                    var nextsong = GetNextSong();

                    Container.KeyCount = (Container.LastKeyId == nextsong.Key.Id)
                        ? Container.KeyCount + 1 : 1;

                    Container.TempoCount = (Container.LastTempoId == nextsong.Tempo.Id)
                        ? Container.TempoCount + 1 : 1;

                    Container.GenreCount = (Container.LastGenreId == nextsong.Genre.Id)
                        ? Container.GenreCount + 1 : 1;

                    _setSongs.Add(new SetSong
                    {
                        SetNumber = Container.CurrentSetNumber,
                        Sequence = Container.SongSequence,
                        Song = nextsong
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
                    .ToDictionary(x => x.Id, x => x);

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
            var eligibleSongs = GetEligibleUnusedSongs();

            var maxIds = eligibleSongs.Count;
            var eligibleIds = eligibleSongs.Keys.Select(x => x).ToArray();
            var rnd = new Random();
            var idx = rnd.Next(0, maxIds);

            return eligibleSongs[eligibleIds[idx]];
        }

        private static Song GetCloser()
        {
            return ApplyRules();
        }

        private static Song  GetMiddle()
        {
            return ApplyRules();
        }

        private static Song ApplyRules()
        {
            var rnd = new Random();
            var rank = 0;
            var numAttempts = 0;

            var eligibleSongs = GetEligibleUnusedSongs();
            var maxIds = eligibleSongs.Count;
            var eligibleIds = eligibleSongs.Keys.Select(x => x).ToArray();

            Song song;

            while (true)
            {
                numAttempts++;
                var idx = rnd.Next(0, maxIds);

                song = eligibleSongs[eligibleIds[idx]]; 

                // apply rules in order of preference
                if ((Container.LastKeyId != song.Key.Id)
                    && (Container.LastTempoId != song.Tempo.Id))
                {
                    if ((Container.GenreCount < 2) &&
                        ((Constants.Genre)song.Genre.Id == Constants.Genre.Country ||
                        (Constants.Genre)song.Genre.Id == Constants.Genre.Rock))
                    {
                        rank = 1;
                    }
                    else if (Container.LastGenreId != song.Genre.Id)
                    {
                        rank = 1;
                    }
                }

                else if ((Container.LastKeyId != song.Key.Id)
                    && (Container.LastTempoId != song.Tempo.Id))
                {
                    rank = 2;
                }

                else if ((Container.LastKeyId != song.Key.Id)
                    && (Container.LastGenreId != song.Genre.Id))
                {
                    rank = 3;
                }

                else if ((Container.LastKeyId != song.Key.Id))
                {
                    rank = 4;
                }

                else if ((Container.LastTempoId != song.Tempo.Id)
                    && (Container.LastKeyId != song.Key.Id))
                {
                    rank = 5;
                }

                // tolerance
                if (rank == 1)
                {
                    break;
                }
                if ((rank == 2) && numAttempts > 100)
                {
                    break;
                }
                if ((rank == 3) && numAttempts > 500)
                {
                    break;
                }
                if ((rank == 4) && numAttempts > 1000)
                {
                    break;
                }
                if ((rank == 5) && numAttempts > 1500)
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