using System;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;
using SetGenerator.Service;

namespace SetGenerator.WebUI.Helpers.SetlistHelpers.Generators
{
    public class KeyGenreTempo
    {
        private Generator _generator;
        private static IEnumerable<Song> _masterSongList;
        private static ICollection<SetSong> _setSongs;

        public IEnumerable<SetSong> Generate(int numSets, int numSongsPerSet, IEnumerable<Song> songs)
        {
            _masterSongList = songs.ToArray();
            _generator = new Generator(_masterSongList);
            _setSongs = _generator.Generate(numSets, numSongsPerSet, GetNextSong);

            return _setSongs;
        }

        private Song GetNextSong()
        {
            SetlistInfo.IsNewSet = (SetlistInfo.SongSequence == 1);
            SetlistInfo.IsLastSong = (SetlistInfo.SongSequence == SetlistInfo.NumSongsPerSet);
            Song nextSong = null;

            //------ FIRST SONG -------
            if (SetlistInfo.IsNewSet)
            {
                nextSong = GetOpener();
            }

            //------ MIDDLE SONG -------
            else if (!SetlistInfo.IsNewSet && !SetlistInfo.IsLastSong)
            {
                nextSong = GetMiddle();
            }

            //------ LAST SONG -------
            else if (SetlistInfo.IsLastSong)
            {
                nextSong = GetCloser();
            }

            return nextSong;
        }

        private Song GetOpener()
        {
            var eligibleSongs = _generator.GetEligibleUnusedSongs();

            var maxIds = eligibleSongs.Count;
            var eligibleIds = eligibleSongs.Keys.Select(x => x).ToArray();
            var rnd = new Random();
            var idx = rnd.Next(0, maxIds);

            return eligibleSongs[eligibleIds[idx]];
        }

        private Song GetCloser()
        {
            var eligibleSongs = _generator.GetEligibleUnusedSongs();
            return ApplyRules(eligibleSongs);
        }

        private Song GetMiddle()
        {
            var eligibleSongs = _generator.GetEligibleUnusedSongs();
            return ApplyRules(eligibleSongs);
        }

        private static Song ApplyRules(IDictionary<int, Song> eligibleSongs)
        {
            var rnd = new Random();
            var rank = 0;
            var numAttempts = 0;

            var maxIds = eligibleSongs.Count;
            var eligibleIds = eligibleSongs.Keys.Select(x => x).ToArray();

            Song song;

            while (true)
            {
                numAttempts++;
                var idx = rnd.Next(0, maxIds);

                song = eligibleSongs[eligibleIds[idx]]; 

                // apply rules in order of preference
                if ((SetlistInfo.LastKeyId != song.Key.Id)
                    && (SetlistInfo.LastTempoId != song.Tempo.Id))
                {
                    if ((SetlistInfo.GenreCount < 2) &&
                        ((Constants.Genre)song.Genre.Id == Constants.Genre.Country ||
                        (Constants.Genre)song.Genre.Id == Constants.Genre.Rock))
                    {
                        rank = 1;
                    }
                    else if (SetlistInfo.LastGenreId != song.Genre.Id)
                    {
                        rank = 1;
                    }
                }

                else if ((SetlistInfo.LastKeyId != song.Key.Id)
                    && (SetlistInfo.LastTempoId != song.Tempo.Id))
                {
                    rank = 2;
                }

                else if ((SetlistInfo.LastKeyId != song.Key.Id)
                    && (SetlistInfo.LastGenreId != song.Genre.Id))
                {
                    rank = 3;
                }

                else if ((SetlistInfo.LastKeyId != song.Key.Id))
                {
                    rank = 4;
                }

                else if ((SetlistInfo.LastTempoId != song.Tempo.Id)
                    && (SetlistInfo.LastKeyId != song.Key.Id))
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