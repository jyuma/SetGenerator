using SetGenerator.Data.Repositories;
using SetGenerator.Domain.Entities;
using SetGenerator.Service;
using SetGenerator.WebUI.Common;
using SetGenerator.WebUI.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;

namespace SetGenerator.WebUI.Controllers
{
    public class SongsController : Controller
    {
        private readonly string _currentUserName;
        private readonly User _currentUser;

        private readonly IBandRepository _bandRepository;
        private readonly ISongRepository _songRepository;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly IValidationRules _validationRules;
        private readonly IAccount _account;
        private readonly CommonSong _common;

        public SongsController( IBandRepository bandRepository, 
                                ISongRepository songRepository, 
                                IInstrumentRepository instrumentRepository,
                                IValidationRules validationRules, 
                                IAccount account)
        {
            _bandRepository = bandRepository;
            _songRepository = songRepository;
            _instrumentRepository = instrumentRepository;
            _validationRules = validationRules;
            _account = account;
            _currentUserName = GetCurrentSessionUser();

            _common = new CommonSong(account, _currentUserName);

            if (_currentUserName.Length > 0)
                _currentUser = _account.GetUserByUserName(_currentUserName);
        }

        public string GetCurrentSessionUser()
        {
            return (System.Web.HttpContext.Current.User.Identity.Name);
        }

        [Authorize]
        public ActionResult Index()
        {
            return View(LoadSongViewModel(0, null));
        }

        private static SongViewModel LoadSongViewModel(int selectedId, List<string> msgs)
        {
            var model = new SongViewModel
            {
                SelectedId = selectedId,
                Success = (msgs == null),
                ErrorMessages = msgs
            };
            return model;
        }

        // for the knockout view model
        [HttpGet]
        public JsonResult GetData()
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var vm = new
            {
                SongList = GetSongList(),
                MemberArrayList = _bandRepository.GetMemberNameArrayList(bandId),
                SingerArrayList = _bandRepository.GetSingerNameArrayList(bandId),
                KeyListFull = GetKeyListFull(),
                GenreArrayList = _songRepository.GetGenreArrayList(),
                TempoArrayList = _songRepository.GetTempoArrayList(),
                TableColumnList = _common.GetTableColumnList(
                    _currentUser.UserPreferenceTableColumns,
                    _currentUser.UserPreferenceTableMembers.Where(x => x.Member.Band.Id == bandId),
                    Constants.UserTable.SongId)
            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<SongDetail> GetSongList()
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var songs = _songRepository.GetByBandId(bandId)
                .ToArray();

            var list = songs.Select(
                x => new SongDetail
                {
                    Id = x.Id,
                    Title = x.Title,
                    KeyId = x.Key.Id,
                    KeyDetail = new SongKeyDetail
                    {
                        Id = x.Key.Id,
                        Name = x.Key.KeyName.Name,
                        NameId = x.Key.KeyName.Id,
                        MajorMinor = x.Key.MajorMinor,
                        SharpFlatNatural = x.Key.SharpFlatNatural
                    },
                    Composer = x.Composer,
                    SingerId = (x.Singer != null) ? x.Singer.Id : 0,
                    GenreId = x.Genre.Id,
                    TempoId = x.Tempo.Id,
                    DateUpdate = x.DateUpdate.ToShortDateString(),
                    UserUpdate = x.UserUpdate.UserName,
                    Disabled = x.IsDisabled,
                    NeverClose = x.NeverClose,
                    NeverOpen = x.NeverOpen,
                    SongMemberInstrumentDetails = x.SongMemberInstruments.Select(y => 
                    new SongMemberInstrumentDetail
                    {
                        MemberId = y.Member.Id,
                        InstrumentId = (y.Instrument != null) ? y.Instrument.Id : 0,
                        InstrumentName = (y.Instrument != null) ? y.Instrument.Name : Constants.SelectListText.NoneSelected
                    }).ToArray()
                }).OrderBy(x => x.Title).ToArray();

            return list;
        }

        private IEnumerable<SongKeyDetail> GetKeyListFull()
        {
            var keys = _songRepository.GetKeyListFull();

            return keys.Select(key => new SongKeyDetail
            {
                Id = key.Id,
                NameId = key.KeyName.Id,
                Name = key.KeyName.Name,
                SharpFlatNatural = key.SharpFlatNatural,
                MajorMinor = key.MajorMinor
            }).ToArray();
        }

        [HttpGet]
        public PartialViewResult GetSongEditView(int id)
        {
            return PartialView("_SongEdit", LoadSongEditViewModel(id));
        }

        private SongEditViewModel LoadSongEditViewModel(int id)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var band = _bandRepository.Get(bandId);
            var song = _songRepository.Get(id);
            var bandMembers = _bandRepository.GetMembers(bandId).ToArray();

            var vm = new SongEditViewModel
            {
                Title = (song != null) ? song.Title : string.Empty,
                Composer = (song != null) ? song.Composer : string.Empty,
                NeverOpen = (song != null) && song.NeverOpen,
                NeverClose = (song != null) && song.NeverClose,
                Composers = _songRepository.GetComposerList(bandId),

                KeyNames = new SelectList(
                    _songRepository.GetKeyNameArrayList(), 
                    "Value", "Display", (song != null) ? song.Key.Id : 0),

                MajorMinor = new SelectList(new Collection<object>
                {
                    new { Value = 0 , Display = "Major" },
                    new { Value = 1 , Display = "Minor" }
                }, "Value", "Display", (song != null) ? song.Key.MajorMinor : 0),

                SharpFlatNatural = new SelectList(new Collection<object>
                {
                    new { Value = 0 , Display = " " },
                    new { Value = 1 , Display = "#" },
                    new { Value = 2 , Display = "b" }
                }, "Value", "Display", (song != null) ? song.Key.SharpFlatNatural : 0),

                Singers = new SelectList(
                    new Collection<object>{ new { Value = 0, Display = "<None>" }}.ToArray()
                        .Union(bandMembers
                        .Select(m => new { Value = m, Display = m })).ToArray()
                    , "Value", "Display", (song != null) 
                        ? (song.Singer != null) 
                            ? song.Singer.Id 
                            : 0
                        : (band.DefaultSinger) != null ? band.DefaultSinger.Id : 0),

                Genres = new SelectList(_songRepository.GetAllGenres()
                         .Select(m => new { Value = m.Id, Display = m.Name }), 
                         "Value", "Display", (song != null) ? song.Genre.Id : 0),

                Tempos = new SelectList(_songRepository.GetAllTempos()
                        .Select(m => new { Value = m.Id, Display = m.Name }),
                        "Value", "Display", (song != null) ? song.Tempo.Id : 0),

                MemberInstruments = bandMembers
                    .Select(m =>
                        new MemberInstrumentDetail
                        {
                            MemberId = m.Id,
                            MemberName = m.FirstName,
                            Instruments = new SelectList(
                                new Collection<object> { new
                                {
                                    Value = 0, 
                                    Display = Constants.SelectListText.NoneSelected
                                } }.ToArray()
                                .Union(m.MemberInstruments
                                .Select(smi => new
                                {
                                    Value = smi.Instrument.Id, 
                                    Display = smi.Instrument.Name
                                })).ToArray(),
                                "Value", "Display", (song != null) 
                                    ? song.SongMemberInstruments.Any(x => x.Member.Id == m.Id) 
                                        ? song.SongMemberInstruments.Single(x => x.Member.Id == m.Id).Instrument.Id
                                        : 0
                                    : m.DefaultInstrument.Id)
                        })
                };

            return vm;
        }

        [HttpPost]
        public JsonResult Save(string song)
        {
            var s = JsonConvert.DeserializeObject<SongDetail>(song);
            List<string> msgs;
            var songId = s.Id;

            if (songId > 0)
            {
                msgs = ValidateSong(s.Title, false);
                if (msgs == null)
                    UpdateSong(s);
            }
            else
            {
                msgs = ValidateSong(s.Title, true);
                if (msgs == null)
                    songId = AddSong(s);
            }

            return Json(new
            {
                SongList = GetSongList(),
                SelectedId = songId,
                Success = (null == msgs),
                ErrorMessages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            _songRepository.Delete(id);

            return Json(new
            {
                SongList = GetSongList(),
                Success = true
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SetDisabled(int id, bool disabled)
        {
            var s = _songRepository.Get(id);

            if (s != null)
            {
                s.IsDisabled = disabled;
                _songRepository.Update(s);
            }

            return Json(new
            {
                SongList = GetSongList(),
                Success = true
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveColumns(string columns)
        {
            var cList = JsonConvert.DeserializeObject<IList<TableColumnDetail>>(columns);
            var cols = new OrderedDictionary();
            foreach (var c in cList)
                cols.Add(c.Data, c.IsVisible);
            _account.UpdateUserTablePreferences(_currentUserName, Constants.UserTable.SongId, cols);
            return Json(JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public List<string> ValidateSong(string title, bool addNew)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            return _validationRules.ValidateSong(bandId, title, addNew);
        }

        // --- for the search dropdown
        [HttpGet]
        public ActionResult GetSingerNameList()
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var list = _bandRepository.GetSingerNameList(bandId);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        // --- for the autocomplete
        [HttpGet]
        public ActionResult GetComposerList()
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var list = _songRepository.GetComposerList(bandId);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        private int AddSong(SongDetail songDetail)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);

            var band = _bandRepository.Get(bandId);
            var members = _bandRepository.GetMembers(bandId).ToArray();
            var instruments = _instrumentRepository.GetAll();
            var genre = _songRepository.GetGenre(songDetail.GenreId);
            var tempo = _songRepository.GetTempo(songDetail.TempoId);

            var newSong = new Song
            {
                Band = band,
                Title = songDetail.Title,
                Singer = songDetail.SingerId > 0
                    ? members.Single(x => x.Id == songDetail.SingerId)
                    : null,
                Key = _songRepository.GetKey(songDetail.KeyId),
                Composer = songDetail.Composer,
                Genre = genre,
                Tempo = tempo,
                NeverClose = songDetail.NeverClose,
                NeverOpen = songDetail.NeverOpen,
                IsDisabled = songDetail.Disabled,
                UserCreate = _currentUser,
                UserUpdate = _currentUser,
                DateCreate = DateTime.Now,
                DateUpdate = DateTime.Now,
                SongMemberInstruments = new List<SongMemberInstrument>()
            };

            foreach (var mi in songDetail.SongMemberInstrumentDetails
                .Where(mi => mi.InstrumentId > 0))
            {
                newSong.SongMemberInstruments.Add(
                    new SongMemberInstrument
                    {
                        Song = newSong,
                        Member = members.Single(x => x.Id == mi.MemberId),
                        Instrument = instruments.Single(x => x.Id == mi.InstrumentId)
                    });
            }

            return _songRepository.Add(newSong);
        }

        private void UpdateSong(SongDetail songDetail)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);

            var song = _songRepository.Get(songDetail.Id);
            var members = _bandRepository.GetMembers(bandId).ToArray();
            var instruments = _instrumentRepository.GetAll();
            var genre = _songRepository.GetGenre(songDetail.GenreId);
            var tempo = _songRepository.GetTempo(songDetail.TempoId);

            if (song != null)
            {
                song.Title = songDetail.Title;
                song.Singer = songDetail.SingerId > 0
                    ? members.Single(x => x.Id == songDetail.SingerId)
                    : null;
                song.Key = _songRepository.GetKey(songDetail.KeyId);
                song.Composer = songDetail.Composer;
                song.Genre = genre;
                song.Tempo = tempo;
                song.NeverClose = songDetail.NeverClose;
                song.NeverOpen = songDetail.NeverOpen;
                song.IsDisabled = songDetail.Disabled;
                song.UserUpdate = _currentUser;
                song.DateUpdate = DateTime.Now;

                song.SongMemberInstruments.Clear();

                foreach (var mi in songDetail.SongMemberInstrumentDetails
                    .Where(mi => mi.InstrumentId > 0))
                {
                    song.SongMemberInstruments.Add(new SongMemberInstrument
                    {
                        Song = song,
                        Member = members.Single(x => x.Id == mi.MemberId),
                        Instrument = instruments.Single(x => x.Id == mi.InstrumentId)
                    });
                }
            };

            _songRepository.Update(song);
        }
    }
}


