using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SetGenerator.Domain.Entities;

namespace SetGenerator.WebUI.Helpers.Algorithms
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

            public static int KeyCount { get; set; }
            public static int LastKeyId { get; set; }

            public static int TempoCount { get; set; }
            public static int LastTempoId { get; set; }

            public static int GenreCount { get; set; }
            public static int LastGenreId { get; set; }

            public static int MaxNumIds { get; set; }
            public static int[] ValidIds { get; set; }
            public static Dictionary<int, Song> DicSongs { get; set; }
        }

        public static IEnumerable<SetSong> Generate(int numSets, int numSongsPerSet, IEnumerable<Song> songs)
        {
            _masterSongList = songs.ToArray();
            
            Container.NumSets = numSets;
            Container.NumSongsPerSet = numSongsPerSet;
            Container.KeyCount = 0;
            Container.LastKeyId = 0;
            Container.TempoCount = 0;
            Container.LastTempoId = 0;
            Container.GenreCount = 0;
            Container.LastGenreId = 0;
            Container.MaxNumIds = 0;
            Container.ValidIds = null;

            _setSongs = new Collection<SetSong>();

            for (Container.CurrentSetNumber = 1; Container.CurrentSetNumber <= Container.NumSets; Container.CurrentSetNumber++)
            {
                // for each set song
                for (var song = 1; song <= Container.NumSongsPerSet; song++)
                {
                    Container.SongSequence++;
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
            int selectedIndex;
            var isNewSet = (Container.SongSequence == 1);
            var isLastSong = (Container.SongSequence == Container.NumSongsPerSet);
            Container.DicSongs = _masterSongList.ToDictionary(x => x.Id);
            Container.ValidIds = GetValidIds();
            Container.MaxNumIds = Container.ValidIds.GetUpperBound(0) + 1;

            //------ FIRST SONG -------
            if (isNewSet)
            {
                selectedIndex = GetOpener();
            }

            //------ MIDDLE SONG -------
            else if (!isLastSong)
            {
                selectedIndex = GetMiddle();
            }

            //------ LAST SONG -------
            else
            {
                selectedIndex = GetCloser();
            }

            return Container.DicSongs[Container.ValidIds[selectedIndex]];
        }

        private static int[] GetValidIds()
        {
            var eligibleSongs = new List<Song>();
            var usedSongs = _setSongs != null
                ? _setSongs.Select(x => x.Song.Id)
                : new Collection<int>();

            eligibleSongs.AddRange(_masterSongList);

            var validIds = eligibleSongs
                .Where(x => !usedSongs.Contains(x.Id))
                .Select(x => x.Id)
                .ToArray();

            if (validIds.Count() == 0)
                validIds = _masterSongList
                   .Where(x => !usedSongs.Contains(x.Id))
                   .Select(x => x.Id)
                   .ToArray();

            return validIds;
        }

        private static int GetOpener()
        {
            var rnd = new Random();
            int idx;
            var numAttempts = 0;

            while (true)
            {
                numAttempts++;
                idx = rnd.Next(0, Container.MaxNumIds);
                if (Container.DicSongs[Container.ValidIds[idx]].NeverOpen == false)
                    break;
                if (numAttempts == 1000)
                    break;
            }
            return idx;
        }

        private static int GetCloser()
        {
            Dictionary<int, Song> validSongs = Container.DicSongs
                .Where(x => x.Value.NeverClose == false)
                .ToDictionary(x => x.Key, x => x.Value);

            return ApplyRules(validSongs);
        }

        private static int GetMiddle()
        {
            return ApplyRules(Container.DicSongs);
        }

        private static int ApplyRules(IDictionary<int, Song> validSongs)
        {
            var rnd = new Random();
            var rank = 0;
            var numAttempts = 0;
            int idx;

            while (true)
            {
                numAttempts++;
                idx = rnd.Next(0, Container.MaxNumIds);

                var s = validSongs[Container.ValidIds[idx]]; 

                // apply rules in order of preference
                if ((Container.LastKeyId != s.Key.Id)
                    && (Container.LastTempoId != s.Tempo.Id)
                    && (Container.LastGenreId != s.Genre.Id))
                {
                    rank = 1;
                }

                else if ((Container.LastKeyId != s.Key.Id)
                    && (Container.LastTempoId != s.Tempo.Id))
                {
                    rank = 2;
                }

                else if ((Container.LastKeyId != s.Key.Id)
                    && (Container.LastGenreId != s.Genre.Id))
                {
                    rank = 3;
                }

                else if ((Container.LastKeyId != s.Key.Id))
                {
                    rank = 4;
                }

                else if ((Container.LastTempoId != s.Tempo.Id)
                    && (Container.LastKeyId != s.Key.Id))
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
            }
            return idx; 
        }
    }
}