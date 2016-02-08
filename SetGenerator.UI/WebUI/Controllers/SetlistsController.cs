﻿using SetGenerator.Service;
using SetGenerator.WebUI.Common;
using SetGenerator.WebUI.ViewModels;
using SetGenerator.Data.Repositories;
using SetGenerator.Domain.Entities;
using SetGenerator.WebUI.Reports.SetsTableAdapters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using SetGenerator.WebUI.Helpers.SetlistHelpers;
using Constants = SetGenerator.Service.Constants;

namespace SetGenerator.WebUI.Controllers
{
    [RoutePrefix("Setlists")]
    public class SetlistsController : Controller
    {
        private readonly IBandRepository _bandRepository;
        private readonly ISetlistRepository _setlistRepository;
        private readonly ISetSongRepository _setSongRepository;
        private readonly ISongRepository _songRepository;
        private readonly IValidationRules _validationRules;

        private readonly User _currentUser;
        private readonly CommonSong _common;

        public SetlistsController(  IBandRepository bandRepository, 
                                    ISetlistRepository setlistRepository,
                                    ISetSongRepository setSongRepository,
                                    ISongRepository songRepository, 
                                    IAccount account, 
                                    IValidationRules validationRules)
        {
            var currentUserName = GetCurrentSessionUser();
            _currentUser = (currentUserName.Length > 0) ? account.GetUserByUserName(currentUserName) : null;

            _bandRepository = bandRepository;
            _setlistRepository = setlistRepository;
            _setSongRepository = setSongRepository;
            _songRepository = songRepository;
            _validationRules = validationRules;
            _common = new CommonSong(account, currentUserName);
        }

        [Authorize]
        public ActionResult Index(int? id)
        {
            return View(id != null
                ? LoadSetlistViewModel(((int) id), null)
                : LoadSetlistViewModel(0, null));
        }

        [Authorize]
        [HttpGet]
        public FileResult Print(int id)
        {
            var rv = new ReportViewer {ProcessingMode = ProcessingMode.Local};
            var ds = new GetSetlistTableAdapter();
            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ReportConnectionString"].ConnectionString);
            ds.Connection = connection;

            string reportPath = GetReportPath();

            rv.LocalReport.ReportPath = Server.MapPath(reportPath);
            rv.ProcessingMode = ProcessingMode.Local;

            var userId = Convert.ToInt32(System.Web.HttpContext.Current.User.Identity.GetUserId());
            ReportDataSource rds = new ReportDataSource("Sets", (object)ds.GetData(id, userId));
            rv.LocalReport.DataSources.Add(rds);
            rv.LocalReport.Refresh();

            string mimeType = string.Empty;
            string encoding = string.Empty;
            string filenameExtension = string.Empty;
            string[] streamids;
            Warning[] warnings;
            var streamBytes = rv.LocalReport.Render("PDF", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);

            var setlist = _setlistRepository.Get(id);
            var filename = setlist.Name + ".pdf";

            return File(streamBytes, mimeType, filename);
        }

        private string GetReportPath()
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var memberIds = _bandRepository.GetMembers(bandId).Select(x => x.Id);
            var reportPath = "~/Reports/Sets.rdlc";

            var showKey = _common.GetTableColumnList(_currentUser.Id, Constants.UserTable.SetId, bandId)
                .Single(x => x.TableColumnId == Constants.UserTableColumn.KeyId)
                .IsVisible;

            var showSinger = _common.GetTableColumnList(_currentUser.Id, Constants.UserTable.SetId, bandId)
                .Single(x => x.TableColumnId == Constants.UserTableColumn.SingerId)
                .IsVisible;

            var showMembers = _currentUser.UserPreferenceTableMembers
                .Where(x => x.Table.Id == Constants.UserTable.SetId)
                .Where(x => memberIds.Contains(x.Member.Id))
                .Any(x => x.IsVisible);

            if (showKey && !showSinger && !showMembers)
            {
                reportPath = "~/Reports/Sets_Key.rdlc";
            }
            else if (showKey && showSinger && !showMembers)
            {
                reportPath = "~/Reports/Sets_Key_Singer.rdlc";
            }
            else if (showKey && !showSinger && showMembers)
            {
                reportPath = "~/Reports/Sets_Key_Members.rdlc";
            }
            else if (!showKey && showSinger && showMembers)
            {
                reportPath = "~/Reports/Sets_Singer_Members.rdlc";
            }
            else if (!showKey && !showSinger && showMembers)
            {
                reportPath = "~/Reports/Sets_Members.rdlc";
            }
            else if (!showKey && showSinger && !showMembers)
            {
                reportPath = "~/Reports/Sets_Singer.rdlc";
            }
            else if (showKey && showMembers && showSinger)
            {
                reportPath = "~/Reports/Sets_Key_Singer_Members.rdlc";
            }

            return reportPath;
        }

        [HttpGet]
        public JsonResult GetData()
        {
            var bandId = Convert.ToInt32(Session["BandId"]);

            var vm = new
            {
                SetlistList = GetSetlistList(),
                TableColumnList = _common.GetTableColumnList(_currentUser.Id, Constants.UserTable.SetlistId, bandId)
            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        private SetlistViewModel LoadSetlistViewModel(int selectedId, List<string> msgs)
        {
            var selectedOwnerSearch = Session["OwnerSearch"] != null 
                ? Session["OwnerSearch"].ToString() 
                : string.Empty;

            var model = new SetlistViewModel
            {
                CurrentUser = _currentUser.UserName,
                SelectedId = selectedId,
                SelectedOwnerSearch = selectedOwnerSearch,
                Success = (msgs == null),
                ErrorMessages = msgs
            };

            return model;
        }

        private IEnumerable<SetlistDetail> GetSetlistList()
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var setlistList = _setlistRepository.GetByBandId(bandId);

            var result = setlistList.Select(setlist => new SetlistDetail
            {
                Id = setlist.Id,
                Name = setlist.Name,
                Owner = setlist.UserCreate.UserName,
                UserUpdate = setlist.UserUpdate.UserName,
                DateUpdate = setlist.DateUpdate.ToShortDateString(),
                NumSets = 1
            }).OrderBy(x => x.Name).ToArray();

            return result;
        }

        [HttpGet]
        public PartialViewResult GetSetlistEditView(int id)
        {
            return PartialView("_SetlistEdit", LoadSetlistEditViewModel(id));
        }

        private SetlistEditViewModel LoadSetlistEditViewModel(int id)
        {
            Setlist setlist = null;

            if (id > 0)
            {
                setlist = _setlistRepository.Get(id);
            }
            var vm = new SetlistEditViewModel
            {
                SetlistId = id,
                Name = (setlist != null) ? setlist.Name : string.Empty,
                TotalSetsList = new SelectList(new Collection<int> { 1, 2, 3 },
                    (setlist != null)
                    ? setlist.SetSongs.Max(x => x.SetNumber) 
                    : 3),
                TotalSongsPerSetlist = new SelectList(new Collection<int> { 8, 9, 10, 11, 12, 13, 14, 15 }, 
                    (setlist != null)
                    ? setlist.SetSongs.Count(x => x.SetNumber == 1)
                    : 10)
            };

            return vm;
        }

        [Route("{id}/Sets/")]
        [Authorize]
        public ActionResult Sets(int id)
        {
            return View(LoadSetViewModel(id, null));
        }

        private SetViewModel LoadSetViewModel(int setlistId, List<string> msgs)
        {
            var model = new SetViewModel
            {
                SetlistId = setlistId,
                Name = _setlistRepository.Get(setlistId).Name,
                Success = (msgs == null),
                ErrorMessages = msgs
            };

            return model;
        }

        [HttpGet]
        public JsonResult GetDataSets(int setlistId)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var setlist = _setlistRepository.Get(setlistId);

            var vm = new
            {
                Name = setlist.Name,
                SetSongList = GetSetSongList(setlist),
                SpareList = GetSpareList(setlist),
                MemberArrayList = _bandRepository.GetMemberNameArrayList(bandId),
                GenreArrayList = _songRepository.GetGenreArrayList(),
                TempoArrayList = _songRepository.GetTempoArrayList(),
                SetNumberList = setlist.SetSongs.Select(x => x.SetNumber).Distinct().ToArray(),
                TableColumnList = _common.GetTableColumnList(_currentUser.Id, Constants.UserTable.SetId, bandId)
            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public PartialViewResult GetAddSongToSetView(int totalSets)
        {
            return PartialView("_AddSongToSet", LoadAddSongToSetViewModel(totalSets));
        }

        private static AddSongToSetViewModel LoadAddSongToSetViewModel(int totalSets)
        {
            var setNumberList = new Collection<int>();
            for (var n = 1; n <= totalSets; n++)
            {
                setNumberList.Add(n);
            }

            var vm = new AddSongToSetViewModel
            {
                TotalSetsList = new SelectList(setNumberList
                        .Select(x => new { Value = x, Display = x }),
                        "Value", "Display"),
            };

            return vm;
        }


        public string GetCurrentSessionUser()
        {
            return (System.Web.HttpContext.Current.User.Identity.Name);
        }

        [HttpPost]
        public JsonResult Save(string setlistDetail)
        {
            var detail = JsonConvert.DeserializeObject<SetlistDetail>(setlistDetail);
            List<string> msgs = null;
            var setlistId = detail.Id;

            if (setlistId > 0)
            {
                msgs = ValidateSetlist(detail.Name, (detail.NumSets * detail.NumSongs), false);
                if (msgs == null)
                {
                    UpdateSetlist(detail);
                }
            }

            return Json(new
            {
                SetlistList = GetSetlistList(),
                SelectedId = setlistId,
                Success = (null == msgs),
                ErrorMessages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Generate(string setlistDetail)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var detail = JsonConvert.DeserializeObject<SetlistDetail>(setlistDetail);
            int selectedId = detail.Id;

            List<string> msgs = ValidateSetlist(detail.Name, (detail.NumSets * detail.NumSongs), true);
            if (msgs == null)
            {
                var generator = new SetlistHelper(_bandRepository);
                var songs = _songRepository.GetAList(bandId);
                var setlist = generator.GenerateSets(songs, detail, bandId, _currentUser);
                selectedId = _setlistRepository.Add(setlist);
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
            _setlistRepository.Delete(id);

            return Json(new
            {
                SetlistList = GetSetlistList(),
                Success = true,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteSetSong(int setlistId, int songId)
        {
            var setSong = _setSongRepository.GetBySetlistSong(setlistId, songId);
            _setSongRepository.Delete(setSong.Id);

            var setlist = _setlistRepository.Get(setlistId);

            return Json(new
            {
                SetSongList = GetSetSongList(setlist),
                SpareList = GetSpareList(setlist),
                Success = true,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddSongToSet(int setlistId, int setNumber, int songId)
        {
            var setlist = _setlistRepository.Get(setlistId);
            var song = _songRepository.Get(songId);

            var sequence = setlist.SetSongs
                .Where(x => x.SetNumber == setNumber)
                .Max(m => m.Sequence) + 1;

            setlist.SetSongs.Add(new SetSong
            {
                Sequence = sequence,
                Setlist = setlist,
                SetNumber = setNumber,
                Song = song
            });

            _setlistRepository.Update(setlist);

            return Json(new
            {
                SetSongList = GetSetSongList(setlist),
                SpareList = GetSpareList(setlist),
                Success = true,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void StoreSelectedOwnerSearch(string ownerSearch)
        {
            Session["OwnerSearch"] = ownerSearch;
        }

        [HttpPost]
        public JsonResult SaveColumns(string columns)
        {
            _common.SaveColumns(columns, Constants.UserTable.SetlistId);
            return Json(JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveColumnsSet(string columns)
        {
            _common.SaveColumns(columns, Constants.UserTable.SetId);
            return Json(JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public List<string> ValidateSetlist(string name, int numSongs, bool addNew)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            return _validationRules.ValidateSetlist(bandId, name, numSongs, addNew);
        }

        private IEnumerable<SetSongDetail> GetSpareList(Setlist setlist)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var setSongIds = setlist.SetSongs.Select(x => x.Song.Id);
            var allAListSongs = _songRepository.GetAList(bandId);

            return allAListSongs
                .Where(x => !setSongIds.Contains(x.Id)).Select(x =>
            {
                var songMemberInstrumentList = GetSongMemberInstrumentDetails(x.SongMemberInstruments);
                return new SetSongDetail
                {
                    SetNumber = 0,
                    Id = x.Id,
                    Title = x.Title,
                    KeyId = x.Key.Id,
                    KeyDetail = GetSongKeyDetail(x.Key),
                    SingerId = (x.Singer != null) ? x.Singer.Id : 0,
                    Composer = x.Composer,
                    GenreId = x.Genre.Id,
                    TempoId = x.Tempo.Id,
                    NeverClose = x.NeverClose,
                    NeverOpen = x.NeverOpen,
                    Disabled = x.IsDisabled,
                    SongMemberInstrumentDetails = songMemberInstrumentList
                };
            }).ToArray();
        }

        private static IEnumerable<SetSongDetail> GetSetSongList(Setlist setlist)
        {
            return setlist.SetSongs.Select(x =>
            {
                var songMemberInstrumentList = GetSongMemberInstrumentDetails(x.Song.SongMemberInstruments);

                return new SetSongDetail
                {
                    SetNumber = x.SetNumber,
                    Id = x.Song.Id,
                    Title = x.Song.Title,
                    KeyId = x.Song.Key.Id,
                    KeyDetail = GetSongKeyDetail(x.Song.Key),
                    SingerId = (x.Song.Singer != null) ? x.Song.Singer.Id : 0,
                    Composer = x.Song.Composer,
                    GenreId = x.Song.Genre.Id,
                    TempoId = x.Song.Tempo.Id,
                    NeverClose = x.Song.NeverClose,
                    NeverOpen = x.Song.NeverOpen,
                    Disabled = x.Song.IsDisabled,
                    SongMemberInstrumentDetails = songMemberInstrumentList
                };
            }).ToArray();
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

        private int AddSetlist(SetlistDetail setlist)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var band = _bandRepository.Get(bandId);

            var sl = new Setlist
            {
                Name = setlist.Name,
                Band = band,
                UserCreate = _currentUser,
                UserUpdate = _currentUser,
                DateCreate = DateTime.Now,
                DateUpdate = DateTime.Now
            };

            return _setlistRepository.Add(sl);
        }

        private void UpdateSetlist(SetlistDetail setlistDetail)
        {
            var setlist = _setlistRepository.Get(setlistDetail.Id);
            if (setlist != null)
            {
                setlist.Name = setlistDetail.Name;
                setlist.UserUpdate = _currentUser;
                setlist.DateUpdate = DateTime.Now;
            };
            _setlistRepository.Update(setlist);
        }

        private static IEnumerable<SongMemberInstrumentDetail> GetSongMemberInstrumentDetails(IEnumerable<SongMemberInstrument> memberInstrumentList)
        {
            return memberInstrumentList.Select(imd => new SongMemberInstrumentDetail
            {
                MemberId = imd.Member.Id,
                InstrumentId = imd.Instrument.Id > 0 ? imd.Instrument.Id : 0,
                InstrumentAbbrev = imd.Instrument != null ? imd.Instrument.Abbreviation : "--"
            }).ToArray();
        }
    }
}
