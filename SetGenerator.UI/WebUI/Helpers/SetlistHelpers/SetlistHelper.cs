using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SetGenerator.Data.Repositories;
using SetGenerator.Domain.Entities;
using SetGenerator.Service;
using SetGenerator.WebUI.Helpers.SetlistHelpers.Algorithms;
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
            Setlist setlist = null;

            if (masterSongList != null)
            {
                var songs = masterSongList.ToArray();
                var numSets = setlistDetail.NumSets;
                var numSetSongs = setlistDetail.NumSongs;

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

                IEnumerable<SetSong> setSongs = null;
                switch (bandId)
                {
                    case Constants.Band.Slugfest:
                    {
                        setSongs = Slugfest.Generate(numSets, numSetSongs, songs);
                        break;
                    }

                    case Constants.Band.TheBeadles:
                    {
                        setSongs = Slugfest.Generate(numSets, numSetSongs, songs);
                        break;
                    }

                    case Constants.Band.StetsonBrothers:
                    {
                        setSongs = StetsonBrothers.Generate(numSets, numSetSongs, songs);
                        break;
                    }
                }

                if (setSongs != null)
                {
                    foreach (var setSong in setSongs)
                    {
                        setSong.Setlist = setlist;
                        setlist.SetSongs.Add(setSong);
                    }
                }
            }

            return setlist;
        }
    }
}