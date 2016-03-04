using System;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Domain.Entities;
using SetGenerator.Service;
using SetGenerator.WebUI.Extensions;

namespace SetGenerator.WebUI.Helpers.SetlistHelpers.Generators
{
    public class InstrumentSinger
    {
        private Generator _generator;
        private static IEnumerable<Song> _masterSongList;
        private static ICollection<SetSong> _setSongs;
        private static IEnumerable<SongMemberInstrument> _songMemberInstrumentList;

        public IEnumerable<SetSong> Generate(int numSets, int numSongsPerSet, IEnumerable<Song> songs)
        {
            _masterSongList = songs.ToArray();
            _songMemberInstrumentList = _masterSongList.SelectMany(x => x.SongMemberInstruments).ToList();
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
            var rnd = new Random();
            var eligibleSongs = _generator.GetEligibleUnusedSongs();
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

                if (numAttempts >= 10) break;
            }

            return eligibleSongs[eligibleIds[idx]];
        }

        private Song GetCloser()
        {
            var eligibleSongs = _generator.GetEligibleUnusedSongs();
            var matchingSongs = SetlistInfo.LastSong.GetMatchingSongs(_songMemberInstrumentList);
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

        private Song GetMiddle()
        {
            var eligibleSongs = _generator.GetEligibleUnusedSongs();
            IDictionary<int, Song> middleEligibleSongs;
            var matchingSongs = SetlistInfo.LastSong.GetMatchingSongs(_songMemberInstrumentList);

            var dicPreviousMatchingSongs = matchingSongs
                .Where(x => x.IsDisabled == false)
                .ToDictionary(x => x.Id, x => x);

            if (SetlistInfo.IntrumentCount < Constants.MinSongInstrumentationCount)
            {
                SetlistInfo.IsSameInstrumentation = true;

                // try to find the same instrumentation as the last song
                middleEligibleSongs = eligibleSongs.Where(x => dicPreviousMatchingSongs
                    .ContainsKey(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                SetlistInfo.IsSameInstrumentation = false;

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

            var eligibleIds = new int[0];

            // if a middle song and an instrument change try to find one that has 
            // at least 3 other songs with the same instrumentation
            if ((!SetlistInfo.IsLastSong) && (SetlistInfo.IsSameInstrumentation == false))
            {
                eligibleIds = eligibleSongs
                    .Where(x => x.Value.GetMatchingSongs(_songMemberInstrumentList).Count() >= Constants.MaxDefaultSingerCount)
                    .Select(x => x.Key).ToArray();

                if (eligibleIds.Count() == 0)
                {
                    eligibleIds = eligibleSongs
                        .Where(x => x.Value.GetMatchingSongs(_songMemberInstrumentList).Count() >= Constants.MaxDefaultSingerCount - 1)
                        .Select(x => x.Key).ToArray();
                }
            }

            if (eligibleIds.Count() == 0)
            {
                eligibleIds = eligibleSongs.Keys.ToArray();
            }

            var maxIds = eligibleIds.Count();

            while (true)
            {
                numAttempts++;
                var idx = rnd.Next(0, maxIds);

                song = eligibleSongs[eligibleIds[idx]];

                if ((SetlistInfo.LastSingerId == SetlistInfo.DefaultSingerId) && (SetlistInfo.SingerCount < Constants.MaxDefaultSingerCount))
                {
                    rank = 1;
                }

                else if ((SetlistInfo.LastSingerId != SetlistInfo.DefaultSingerId) && (SetlistInfo.LastSingerId != (song.Singer != null ? song.Singer.Id : 0)))
                {
                    rank = 1;
                }

                else if ((SetlistInfo.LastSingerId == SetlistInfo.DefaultSingerId) && (SetlistInfo.SingerCount < Constants.MaxDefaultSingerCount))
                {
                    rank = 2;
                }

                else if ((SetlistInfo.LastSingerId != SetlistInfo.DefaultSingerId) && (SetlistInfo.LastSingerId != (song.Singer != null ? song.Singer.Id : 0)))
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