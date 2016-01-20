using Newtonsoft.Json;
using SetGenerator.Domain;
using SetGenerator.Repositories;
using SetGenerator.Service;
using SetGenerator.WebUI.Common;
using SetGenerator.WebUI.Extensions;
using SetGenerator.WebUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Reporting.WebForms;
using SetGenerator.WebUI.Reports.SetsTableAdapters;
using Constants = SetGenerator.Service.Constants;

namespace SetGenerator.WebUI.Controllers
{
    [RoutePrefix("SetLists")]
    public class SetListsController : Controller
    {
        private readonly ISetListRepository _setListRepository;
        private readonly ISongRepository _songRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IValidationRules _validationRules;

        private readonly User _currentUser;
        private readonly CommonSong _common;

        public SetListsController(  ISetListRepository setListRepository, 
                                    ISongRepository songRepository, 
                                    IMemberRepository memberRepository, 
                                    IKeyRepository keyRepository,
                                    IAccount account, 
                                    IValidationRules validationRules)
        {
            var currentUserName = GetCurrentSessionUser();
            _currentUser = (currentUserName.Length > 0) ? account.GetUserByUserName(currentUserName) : null;

            _setListRepository = setListRepository;
            _songRepository = songRepository;
            _memberRepository = memberRepository;
            _validationRules = validationRules;
            _common = new CommonSong(account, keyRepository, memberRepository, currentUserName);
        }

        [Authorize]
        public ActionResult Index()
        {
            return View(LoadSetListViewModel(0, null));
        }

        [Authorize]
        [HttpGet]
        public FileResult Print(int id)
        {
            var rv = new ReportViewer {ProcessingMode = ProcessingMode.Local};
            var ds = new GetSetListTableAdapter();
            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ReportConnectionString"].ConnectionString);
            ds.Connection = connection;

            string mimeType = string.Empty;
            string encoding = string.Empty;
            string filenameExtension = string.Empty;
            string[] streamids;
            Warning[] warnings;

            rv.LocalReport.ReportPath = Server.MapPath("~/Reports/Sets.rdlc");
            rv.ProcessingMode = ProcessingMode.Local;

            var userId = Convert.ToInt32(System.Web.HttpContext.Current.User.Identity.GetUserId());
            ReportDataSource rds = new ReportDataSource("Sets", (object)ds.GetData(id, userId));
            rv.LocalReport.DataSources.Add(rds);
            rv.LocalReport.Refresh();

            var streamBytes = rv.LocalReport.Render("PDF", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);

            var setlist = _setListRepository.GetSingle(id);
            var filename = setlist.Name + ".pdf";

            return File(streamBytes, mimeType, filename);
        }

        [HttpGet]
        public JsonResult GetData()
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var vm = new
            {
                setlistList = GetSetlistList(),
                tableColumnList = _common.GetTableColumnList(_currentUser.UserPreferenceTableColumns, _currentUser.UserPreferenceTableMembers, Constants.UserTable.SetListId, bandId)
            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDataSets(int setlistId)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var setlist = _setListRepository.GetSingle(setlistId);

            var vm = new
            {
                name = setlist.Name,
                songList = GetSongList(setlist),
                unusedSongList = GetUnusedSongList(setlist),
                memberArrayList = _memberRepository.GetNameArrayList(bandId),
                setNumberList = setlist.Sets.Select(x => x.Number).Distinct().ToArray(),
                tableColumnList = _common.GetTableColumnList(_currentUser.UserPreferenceTableColumns, _currentUser.UserPreferenceTableMembers, Constants.UserTable.SetId, bandId)
            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        private SetViewModel LoadSetViewModel(int selectedId, List<string> msgs)
        {
            var model = new SetViewModel
            {
                SetListId = selectedId,
                Name = _setListRepository.GetSingle(selectedId).Name,
                Success = (msgs == null),
                ErrorMessages = msgs
            };

            return model;
        }

        private static SetListViewModel LoadSetListViewModel(int selectedId, List<string> msgs)
        {
            var model = new SetListViewModel
                        {
                            SelectedId = selectedId, 
                            Success = (msgs == null), 
                            ErrorMessages = msgs
                        };

            return model;
        }

        private IEnumerable<SetListDetail> GetSetlistList()
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var setlistList = _setListRepository.GetList(bandId);

            var result = setlistList.Select(setlist => new SetListDetail
            {
                Id = setlist.Id,
                BandId = setlist.BandId,
                Name = setlist.Name,
                UserUpdate = setlist.User1.UserName,
                DateUpdate = setlist.DateUpdate.ToShortDateString(),
                NumSets = 1
            }).ToArray();

            return result;
        }

        [HttpGet]
        public PartialViewResult GetSetListEditView(int id)
        {
            return PartialView("_SetListEdit", LoadSetListEditViewModel(id));
        }

        private SetListEditViewModel LoadSetListEditViewModel(int id)
        {
            SetList setlist = null;

            if (id > 0)
            {
                setlist = _setListRepository.GetSingle(id);
            }
            var vm = new SetListEditViewModel
            {
                SetListId = id,
                Name = (setlist != null) ? setlist.Name : string.Empty,
                ToalSetsList = new SelectList(new Collection<int> { 1, 2, 3 },
                    (setlist != null) 
                    ? setlist.Sets.Max(x => x.Number) 
                    : 3),
                TotalSongsPerSetList = new SelectList(new Collection<int> { 8, 9, 10, 11, 12, 13, 14, 15 }, 
                    (setlist != null) 
                    ? setlist.Sets.Count(x => x.Number == 1)
                    : 10)
            };

            return vm;
        }

        [Route("{id}/Sets/")]
        public ActionResult Sets(int id)
        {
            return View(LoadSetViewModel(id, null));
        }

        public string GetCurrentSessionUser()
        {
            return (System.Web.HttpContext.Current.User.Identity.Name);
        }

        [HttpPost]
        public JsonResult Save(string setListDetail)
        {
            var detail = JsonConvert.DeserializeObject<SetListDetail>(setListDetail);
            List<string> msgs = null;
            var setListId = detail.Id;

            if (setListId > 0)
            {
                msgs = ValidateSetList(detail.Name, (detail.NumSets * detail.NumSongs), false);
                if (msgs == null)
                    UpdateSetList(detail);
            }
            //var vm = LoadSetListViewModel(setListId, msgs);
            return Json(new
            {
                SetlistList = GetSetlistList(),
                SelectedId = setListId,
                Success = (null == msgs),
                ErrorMessages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Generate(string setListDetail)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var detail = JsonConvert.DeserializeObject<SetListDetail>(setListDetail);
            int selectedId = detail.Id;

            List<string> msgs = ValidateSetList(detail.Name, (detail.NumSets * detail.NumSongs), true);
            if (msgs == null)
            {
                var songs = _songRepository.GetAList(bandId);
                var setlist = songs.GenerateSets(detail, bandId, _currentUser.Id);
                selectedId = _setListRepository.Add(setlist);
            }

            return Json(new
            {
                SetlistList = GetSetlistList(),
                SelectedId = selectedId,
                Success = (null == msgs),
                ErrorMessages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            var sl = _setListRepository.GetSingle(id);

            _setListRepository.Delete(sl);

            return Json(new
            {
                SetlistList = GetSetlistList(),
                Success = true,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveColumns(string columns)
        {
            _common.SaveColumns(columns, Constants.UserTable.SetListId);
            return Json(JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveColumnsSet(string columns)
        {
            _common.SaveColumns(columns, Constants.UserTable.SetId);
            return Json(JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public List<string> ValidateSetList(string name, int numSongs, bool addNew)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            return _validationRules.ValidateSetList(bandId, name, numSongs, addNew);
        }

        private IEnumerable<SetSongDetail> GetUnusedSongList(SetList setlist)
        {
            var result = new Collection<SetSongDetail>();
            var bandId = Convert.ToInt32(Session["BandId"]);

            var usedSongs = setlist.Sets.Select(x => x.SongId);
            var allSongs = _songRepository.GetAList(bandId);
            var unUsedSongs = allSongs
                .Where(x => !usedSongs.Contains(x.Id))
                .ToArray();

            foreach (var song in unUsedSongs)
            {
                var songMemberInstrumentList = GetSongMemberInstrumentDetails(song.SongMemberInstruments);
                result.Add(new SetSongDetail
                {
                    Number = 0,
                    Id = song.Id,
                    BandId = bandId,
                    Title = song.Title,
                    KeyId = song.KeyId,
                    KeyDetail = GetSongKeyDetail(song.Key),
                    SingerId = song.SingerId,
                    Composer = song.Composer,
                    NeverClose = song.NeverClose,
                    NeverOpen = song.NeverOpen,
                    Disabled = song.IsDisabled,
                    UserUpdate = song.User1.UserName,
                    DateUpdate = song.DateUpdate.ToShortDateString(),
                    SongMemberInstrumentDetails = songMemberInstrumentList
                });
            }

            return result;
        }

        private IEnumerable<SetSongDetail> GetSongList(SetList setlist)
        {
            var result = new Collection<SetSongDetail>();
            var bandId = Convert.ToInt32(Session["BandId"]);

            foreach (var set in setlist.Sets)
            {
                var songMemberInstrumentList = GetSongMemberInstrumentDetails(set.Song.SongMemberInstruments);
                result.Add(new SetSongDetail
                {
                    Number = set.Number,
                    Id = set.Song.Id,
                    BandId = bandId,
                    Title = set.Song.Title,
                    KeyId = set.Song.KeyId,
                    KeyDetail = GetSongKeyDetail(set.Song.Key),
                    SingerId = set.Song.SingerId,
                    Composer = set.Song.Composer,
                    NeverClose = set.Song.NeverClose,
                    NeverOpen = set.Song.NeverOpen,
                    Disabled = set.Song.IsDisabled,
                    UserUpdate = set.Song.User1.UserName,
                    DateUpdate = set.Song.DateUpdate.ToShortDateString(),
                    SongMemberInstrumentDetails = songMemberInstrumentList
                });
            }

            return result;
        }

        private static SongKeyDetail GetSongKeyDetail(Key key)
        {
            return new SongKeyDetail
            {
                Id = key.Id,
                NameId = key.KeyName.Id,
                Name = key.KeyName.Name,
                SharpFlatNatural = key.SharpFlatNatural,
                MajorMinor = key.MajorMinor
            };
        }

        private int AddSetList(SetListDetail setList)
        {
            var sl = new SetList
            {
                Name = setList.Name,
                BandId = setList.BandId,
                UserCreateId = _currentUser.Id,
                UserUpdateId = _currentUser.Id,
                DateCreate = DateTime.Now,
                DateUpdate = DateTime.Now
            };

            return _setListRepository.Add(sl);
        }

        private void UpdateSetList(SetListDetail setListDetail)
        {
            var setList = _setListRepository.GetSingle(setListDetail.Id);
            if (setList != null)
            {
                setList.Name = setListDetail.Name;
                setList.UserUpdateId = _currentUser.Id;
                setList.DateUpdate = DateTime.Now;
            };
            _setListRepository.Update();
        }

        private static IEnumerable<SongMemberInstrumentDetail> GetSongMemberInstrumentDetails(IEnumerable<SongMemberInstrument> memberInstrumentList)
        {
            return memberInstrumentList.Select(imd => new SongMemberInstrumentDetail
            {
                MemberId = imd.MemberId,
                InstrumentId = imd.InstrumentId ?? 0,
                InstrumentAbbrev = imd.Instrument != null ? imd.Instrument.Abbreviation : "--"
            }).ToArray();
        }
    }
}
