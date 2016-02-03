using System;
using System.Collections.Generic;
using System.Linq;
using SetGenerator.Data.Repositories;

namespace SetGenerator.Service
{
    public interface IValidationRules
    {
        //-- song
        List<string> ValidateSong(int bandId, string title, bool addNew);
        List<string> ValidateTitle(int bandId, string title, bool addNew, List<string> msgs);

        // setlist
        List<string> ValidateSetlist(int bandId, string name, int numSongs, bool addNew);

        // gig
        List<string> ValidateGig(int bandId, string venue, DateTime dateGig, bool addNew);
    }

    public class ValidationRules : IValidationRules
    {
        private readonly ISongRepository _songRepository;
        private readonly ISetlistRepository _setListRepository;
        private readonly IGigRepository _gigRepository;

        public ValidationRules( ISongRepository songRepository, 
                                ISetlistRepository setListRepository, 
                                IGigRepository gigRepository)
        {
            _songRepository = songRepository;
            _setListRepository = setListRepository;
            _gigRepository = gigRepository;
        }
        //----------------------------------- validation rules ------------------------------------

        // song
        public List<string> ValidateSong(int bandId, string title, bool addNew)
        {
            var msgs = new List<string>();

            msgs = ValidateTitle(bandId, title, addNew, msgs);

            return msgs.Count > 0 ? msgs : null;
        }

        public List<string> ValidateTitle(int bandId, string title, bool addNew, List<string> msgs)
        {
            var s = _songRepository.GetByTitle(bandId, title);

            if (string.IsNullOrEmpty(title))
                msgs.Add("Title is required");
            if (addNew && s != null)
                msgs.Add("Title already exists");

            return msgs;
        }


        // setlist
        public List<string> ValidateSetlist(int bandId, string name, int numSongs, bool addNew)
        {
            var msgs = new List<string>();
            msgs = ValidateName(bandId, name, addNew, msgs);
            msgs = ValidateCount(bandId, numSongs, addNew, msgs);
            return msgs.Count > 0 ? msgs : null;
        }

        private List<string> ValidateName(int bandId, string name, bool addNew, List<string> msgs)
        {
            var s = _setListRepository.GetByName(bandId, name);

            if (string.IsNullOrEmpty(name))
                msgs.Add("Name is required");
            if (addNew && s != null)
                msgs.Add("Name already exists");

            return msgs;
        }

        private List<string> ValidateCount(int bandId, int numSongs, bool addNew, List<string> msgs)
        {
            var songCount = _songRepository.GetAList(bandId).Count();
            if (!addNew) return msgs;
            if (numSongs > songCount)
                msgs.Add("There just aren't enough songs, dude!");
            return msgs;
        }


        // gig
        public List<string> ValidateGig(int bandId, string venue, DateTime dateGig, bool addNew)
        {
            var msgs = new List<string>();
            msgs = ValidateVenue(bandId, venue, dateGig, addNew, msgs);
            return msgs.Count > 0 ? msgs : null;
        }

        private List<string> ValidateVenue(int bandId, string venue, DateTime dateGig, bool addNew, List<string> msgs)
        {
            var s = _gigRepository.GetByBandVenueDateGig(bandId, venue, dateGig);

            if (string.IsNullOrEmpty(venue))
                msgs.Add("Venue is required");
            if (addNew && s != null)
                msgs.Add("Venue / Gig Date combination already exists");

            return msgs;
        }
    }
}
