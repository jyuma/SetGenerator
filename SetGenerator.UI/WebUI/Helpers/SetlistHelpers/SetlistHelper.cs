using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SetGenerator.Data.Repositories;
using SetGenerator.Domain.Entities;
using SetGenerator.WebUI.Helpers.SetlistHelpers.Generators;
using SetGenerator.WebUI.ViewModels;

namespace SetGenerator.WebUI.Helpers.SetlistHelpers
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
            if (masterSongList == null) return null;

            var songs = masterSongList.ToArray();
            var numSets = setlistDetail.NumSets;
            var numSetSongs = setlistDetail.NumSongs;

            var setlist = new Setlist
            {
                Band = _bandRepository.Get(bandId),
                Name = setlistDetail.Name,
                DateCreate = DateTime.Now,
                DateUpdate = DateTime.Now,
                UserCreate = currentUser,
                UserUpdate = currentUser,
                SetSongs = new Collection<SetSong>()
            };

            IEnumerable<SetSong> setSongs;

            var numChanges = GetNumberInstrumentChanges(songs);

            switch (numChanges)
            {
                case 0:
                {
                    var keyGenereTempo = new KeyGenreTempo();
                    setSongs = keyGenereTempo.Generate(numSets, numSetSongs, songs);
                    break;
                }
                    
                default:
                {
                    var instrumentSinger = new InstrumentSinger();
                    setSongs = instrumentSinger.Generate(numSets, numSetSongs, songs);
                    break;
                }
            }

            if (setSongs == null) return setlist;

            foreach (var setSong in setSongs)
            {
                setSong.Setlist = setlist;
                setlist.SetSongs.Add(setSong);
            }

            return setlist;
        }

        private static int GetNumberInstrumentChanges(ICollection<Song> songs)
        {
            var numMembers = songs.SelectMany(x => x.SongMemberInstruments)
                .GroupBy(x => x.Member)
                .Count();
            
            var instrumentationCount = songs.SelectMany(x => x.SongMemberInstruments)
                .GroupBy(x => x.Instrument)
                .Select(x => x.Key)
                .Count();

            return (instrumentationCount - numMembers);
        }
    }
}