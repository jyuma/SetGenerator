using SetGenerator.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace SetGenerator.Service
{
    public interface IValidationRules
    {
        //-- song
        List<string> ValidateSong(int bandId, string title, bool addNew);
        List<string> ValidateTitle(int bandId, string title, bool addNew, List<string> msgs);

        // setlist
        List<string> ValidateSetList(int bandId, string name, int numSongs, bool addNew);
    }

    public class ValidationRules : IValidationRules
    {
        private readonly ISongRepository _songRepository;
        private readonly ISetListRepository _setListRepository;

        public ValidationRules(ISongRepository songRepository, ISetListRepository setListRepository)
        {
            _songRepository = songRepository;
            _setListRepository = setListRepository;
        }
        //----------------------------------- validation rules ------------------------------------

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
                msgs.Add("Title is required, dude.");
            if (addNew && s != null)
                msgs.Add("Title already exists, dude.");

            return msgs;
        }

        public List<string> ValidateSetList(int bandId, string name, int numSongs, bool addNew)
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
                msgs.Add("Name is required, dude.");
            if (addNew && s != null)
                msgs.Add("Name already exists, dude.");

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
    }
}
