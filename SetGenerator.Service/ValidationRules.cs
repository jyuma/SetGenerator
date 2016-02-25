﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SetGenerator.Data.Repositories;

namespace SetGenerator.Service
{
    public interface IValidationRules
    {
        // user
        List<string> ValidateUser(string username, string password, string email, bool addNew);

        // band
        List<string> ValidateBand(string name, bool addNew);

        // member
        List<string> ValidateMember(int bandId, string firstName, string lastName, string alias, bool addNew);

        // song
        List<string> ValidateSong(int bandId, string title, bool addNew);
        List<string> ValidateTitle(int bandId, string title, bool addNew, List<string> msgs);

        // setlist
        List<string> ValidateSetlist(int bandId, string name, int numSongs, bool addNew);

        // gig
        List<string> ValidateGig(int bandId, string venue, DateTime dateGig, bool addNew);
    }

    public class ValidationRules : IValidationRules
    {
        private readonly IBandRepository _bandRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly ISongRepository _songRepository;
        private readonly ISetlistRepository _setListRepository;
        private readonly IGigRepository _gigRepository;

        public ValidationRules(IBandRepository bandRepository,
                                IMemberRepository memberRepository, 
                                ISongRepository songRepository, 
                                ISetlistRepository setListRepository, 
                                IGigRepository gigRepository)
        {
            _bandRepository = bandRepository;
            _memberRepository = memberRepository;
            _songRepository = songRepository;
            _setListRepository = setListRepository;
            _gigRepository = gigRepository;
        }
        //----------------------------------- validation rules ------------------------------------

        // user
        public List<string> ValidateUser(string username, string password, string email, bool addNew)
        {
            var msgs = new List<string>();
            msgs = ValidateUserName(username, addNew, msgs);
            msgs = ValidatePassword(username, msgs);
            msgs = ValidateEmail(email, msgs);

            return msgs.Count > 0 ? msgs : null;
        }

        private List<string> ValidateUserName(string username, bool addNew, List<string> msgs)
        {
            var band = _bandRepository.GetByName(username);

            if (string.IsNullOrEmpty(username))
                msgs.Add("UserName is required");
            if (addNew && band != null)
                msgs.Add("UserName already exists");

            return msgs;
        }

        private static List<string> ValidatePassword(string password, List<string> msgs)
        {
            if (string.IsNullOrEmpty(password))
                msgs.Add("Password is required");

            return msgs;
        }

        private static List<string> ValidateEmail(string email, List<string> msgs)
        {
            const string validEmailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                                             + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                                             + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            var validEmailRegex = new Regex(validEmailPattern, RegexOptions.IgnoreCase);

            var isMatch = validEmailRegex.IsMatch(email);

            if (!isMatch)
                msgs.Add("Invalild email address");

            return msgs;
        }

        // band
        public List<string> ValidateBand(string name, bool addNew)
        {
            var msgs = new List<string>();
            msgs = ValidateName(name, addNew, msgs);
            return msgs.Count > 0 ? msgs : null;
        }

        private List<string> ValidateName(string name, bool addNew, List<string> msgs)
        {
            var band = _bandRepository.GetByName(name);

            if (string.IsNullOrEmpty(name))
                msgs.Add("Name is required");
            if (addNew && band != null)
                msgs.Add("Band name already exists");

            return msgs;
        }

        // member
        public List<string> ValidateMember(int bandId, string firstName, string lastName, string alias, bool addNew)
        {
            var msgs = new List<string>();
            msgs = ValidateAlias(bandId, alias, addNew, msgs);
            return msgs.Count > 0 ? msgs : null;
        }

        private List<string> ValidateAlias(int bandId, string alias, bool addNew, List<string> msgs)
        {
            var member = _memberRepository.GetByBandIdAlias(bandId, alias);

            if (string.IsNullOrEmpty(alias))
                msgs.Add("Alias is required");
            if (addNew && member != null)
                msgs.Add("Alias already exists");

            return msgs;
        }

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
                msgs.Add("Repertoire doesn't contain enough songs");
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
