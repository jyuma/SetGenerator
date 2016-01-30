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

        private readonly ISongRepository _songRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly IKeyRepository _keyRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly ITempoRepository _tempoRepository;
        private readonly IValidationRules _validationRules;
        private readonly IAccount _account;
        private readonly CommonSong _common;

        public SongsController( ISongRepository songRepository, 
                                IMemberRepository memberRepository, 
                                IKeyRepository keyRepository,
                                IInstrumentRepository instrumentRepository,
                                IGenreRepository genreRepository,
                                ITempoRepository tempoRepository,
                                IValidationRules validationRules, 
                                IAccount account)
        {
            _songRepository = songRepository;
            _memberRepository = memberRepository;
            _instrumentRepository = instrumentRepository;
            _genreRepository = genreRepository;
            _tempoRepository = tempoRepository;
            _keyRepository = keyRepository;
            _validationRules = validationRules;
            _account = account;
            _currentUserName = GetCurrentSessionUser();

            _common = new CommonSong(account, keyRepository, memberRepository, _currentUserName);

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

        [HttpGet]
        public JsonResult GetData()
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var vm = new
            {
                songList = GetSongList(),
                memberArrayList = _memberRepository.GetNameArrayList(bandId),
                keyListFull = _common.GetKeyListFull(),
                tableColumnList = _common.GetTableColumnList(_currentUser.UserPreferenceTableColumns, _currentUser.UserPreferenceTableMembers, Constants.UserTable.SongId, bandId)
            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<SongDetail> GetSongList()
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var songs = _songRepository.GetAll().Where(x => x.Band.Id == bandId);

            return songs.Select(
                x => new SongDetail
                {
                    Id = x.Id,
                    BandId = bandId,
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
                    SingerId = x.Singer.Id,
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
                        InstrumentId = y.Instrument.Id,
                        InstrumentName = y.Instrument.Name
                    }).ToArray()
                }).OrderBy(x => x.Title).ToArray();
        }

        [HttpGet]
        public PartialViewResult GetSongEditView(int id)
        {
            return PartialView("_SongEdit", LoadSongEditViewModel(id));
        }

        private SongEditViewModel LoadSongEditViewModel(int id)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var song = _songRepository.Get(id);

            var vm = new SongEditViewModel
            {
                Title = (song != null) ? song.Title : string.Empty,
                Composer = (song != null) ? song.Composer : string.Empty,
                NeverOpen = (song != null) && song.NeverOpen,
                NeverClose = (song != null) && song.NeverClose,
                Composers = _songRepository.GetComposerList(bandId),
                KeyNames = new SelectList(
                    _keyRepository.GetNameArrayList(), 
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

                Members = new SelectList(
                    new Collection<object>{ new { Value = 0, Display = "<None>" }}.ToArray()
                        .Union(_memberRepository.GetAll().Where(x => x.Band.Id == bandId)
                        .Select(m => new { Value = m.Id, Display = m.FirstName })).ToArray()
                    , "Value", "Display", (song != null) ? song.Singer.Id : 0),

                Genres = new SelectList(_genreRepository.GetAll()
                         .Select(m => new { Value = m.Id, Display = m.Name }), 
                         "Value", "Display", (song != null) ? song.Genre.Id : 0),

                Tempos = new SelectList(_tempoRepository.GetAll()
                        .Select(m => new { Value = m.Id, Display = m.Name }),
                        "Value", "Display", (song != null) ? song.Tempo.Id : 0),

                MemberInstruments = _memberRepository.GetAll()
                .Where(x => x.Band.Id == bandId)
                .Select(m =>
                    new MemberInstrumentDetail
                    {
                        MemberId = m.Id,
                        MemberName = m.FirstName,
                        Instruments = new SelectList(
                            new Collection<object> { new { Value = 0, Display = "<None>" } }.ToArray()
                            .Union(m.MemberInstruments
                            .Select(smi => new { Value = smi.Instrument.Id, Display = smi.Instrument.Name })).ToArray(),
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
            //var s = _songRepository.Get(id);

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
        public ActionResult GetMemberNameList()
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var list = _memberRepository.GetNameList(bandId);
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

        private int AddSong(SongDetail song)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);

            var s = new Song
            {
                Title = song.Title,
                //Band.Id = bandId,
                //Member.Id = (song.SingerId > 0 ? song.SingerId : null),
                //Key.Id = song.KeyId,
                Composer = song.Composer,
                NeverClose = song.NeverClose,
                NeverOpen = song.NeverOpen,
                IsDisabled = song.Disabled,
                UserCreateId = _currentUser.Id,
                UserUpdateId = _currentUser.Id,
                DateCreate = DateTime.Now,
                DateUpdate = DateTime.Now
            };

            //foreach (var im in song.SongMemberInstrumentDetails.Where(i => i.InstrumentId > 0))
            //{
            //    s.SongMemberInstruments.Add(new SongMemberInstrument
            //        {
            //            BandId = s.BandId,
            //            InstrumentId = im.InstrumentId,
            //            MemberId = im.MemberId
            //        });
            //}

            return _songRepository.Add(s);
        }

        private void UpdateSong(SongDetail songDetail)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var song = _songRepository.Get(songDetail.Id);
            if (song != null)
            {
                song.Title = songDetail.Title;
                //song.SingerId = (songDetail.SingerId > 0 ? songDetail.SingerId : null);
                //song.KeyId = songDetail.KeyId;
                song.Composer = songDetail.Composer;
                song.NeverClose = songDetail.NeverClose;
                song.NeverOpen = songDetail.NeverOpen;
                song.IsDisabled = songDetail.Disabled;
                song.UserUpdateId = _currentUser.Id;
                song.DateUpdate = DateTime.Now;

                //song.SongMemberInstruments.Clear();

                //foreach (var mi in songDetail.SongMemberInstrumentDetails.Where(i => i.InstrumentId > 0))
                //{
                //    song.SongMemberInstruments.Add(new SongMemberInstrument
                //    {
                //        BandId = bandId,
                //        SongId = song.Id,
                //        MemberId = mi.MemberId,
                //        InstrumentId = mi.InstrumentId
                //    });
                //}
            };

            _songRepository.Update(song);
        }
    }
}


