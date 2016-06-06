using Newtonsoft.Json;
using SetGenerator.Service;
using SetGenerator.WebUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Mvc;
using SetGenerator.Data.Repositories;
using SetGenerator.Domain.Entities;
using SetGenerator.WebUI.Common;

namespace SetGenerator.WebUI.Controllers
{
    [RoutePrefix("Bands")]
    public class BandsController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IBandRepository _bandRepository;
        private readonly ISongRepository _songRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IValidationRules _validationRules;
        private readonly User _currentUser;
        private readonly CommonSong _common;

        public BandsController( IUserRepository userRepository,
                                IBandRepository bandRepository,
                                ISongRepository songRepository,
                                IMemberRepository memberRepository,
                                IValidationRules validationRules,
                                IAccount account)
        {
            _userRepository = userRepository;
            _bandRepository = bandRepository;
            _songRepository = songRepository;
            _memberRepository = memberRepository;
            _validationRules = validationRules;

            var currentUserName = GetCurrentSessionUser();
            if (currentUserName.Length > 0)
                _currentUser = account.GetUserByUserName(currentUserName);
            _common = new CommonSong(account, currentUserName);
        }

        [Authorize]
        public ActionResult Index(int? id)
        {
            return View(id != null
                ? LoadBandViewModel(((int)id), null)
                : LoadBandViewModel(0, null));
        }

        [HttpGet]
        public JsonResult GetData()
        {
            var vm = new
            {
                BandList = GetBandList(),
                DefaultSingerArrayList = _bandRepository.GetDefaultSingerNameArrayList(),
                DefaultGenreArrayList = _bandRepository.GetDefaultGenreArrayList(),
                TableColumnList = _common.GetTableColumnList(_currentUser.Id, Constants.UserTable.BandId)
            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<BandDetail> GetBandList()
        {
            IEnumerable<Band> bandList;

            if (Session["UserName"].ToString().ToLower() != "admin")
            {
                bandList = _userRepository
                    .GetUserBands(_currentUser.Id)
                    .Select(x => x.Band);
            }
            else
            {
                bandList = _bandRepository.GetAll();
            }

            var result = bandList.Select(band => new BandDetail
            {
                Id = band.Id,
                Name = band.Name,
                DefaultSingerId = (band.DefaultSinger != null) ? band.DefaultSinger.Id : 0,
                DefaultGenreId = (band.DefaultGenre != null) ? band.DefaultGenre.Id : 0,
                UserUpdate = band.UserUpdate.UserName,
                DateUpdate = band.DateUpdate.ToShortDateString()
            }).OrderBy(x => x.Name).ToArray();

            return result;
        }

        public string GetCurrentSessionUser()
        {
            return (System.Web.HttpContext.Current.User.Identity.Name);
        }

        private static BandViewModel LoadBandViewModel(int selectedId, List<string> msgs)
        {
            var model = new BandViewModel
            {
                SelectedId = selectedId,
                Success = (msgs == null),
                ErrorMessages = msgs
            };

            return model;
        }

        [HttpGet]
        public PartialViewResult GetBandEditView(int id)
        {
            return PartialView("_BandEdit", LoadBandEditViewModel(id));
        }

        private BandEditViewModel LoadBandEditViewModel(int id)
        {
            Band band = null;

            if (id > 0)
            {
                band = _bandRepository.Get(id);
            }
            var vm = new BandEditViewModel
            {
                Name = (band != null) ? band.Name : string.Empty,
                Members = new SelectList((band != null)
                            ? new Collection<object> { new { Value = "0", Display = "<None>" } }.ToArray()
                            .Union(
                                band.Members
                                .Select(x => new
                                {
                                    Value = x.Id,
                                    Display = x.Alias
                                })).ToArray()
                             : new Collection<object>().ToArray(), "Value", "Display",
                                (band != null) 
                                    ? (band.DefaultSinger != null) ? band.DefaultSinger.Id : 0
                                    : 0),

                Genres = new SelectList((band != null)
                            ? new Collection<object> { new { Value = "0", Display = "<None>" } }.ToArray()
                            .Union(
                                _bandRepository.GetAllGenres()
                                .Select(x => new
                                {
                                    Value = x.Id,
                                    Display = x.Name
                                })).ToArray()
                             : new Collection<object>().ToArray(), "Value", "Display",
                                (band != null)
                                    ? (band.DefaultGenre != null) ? band.DefaultGenre.Id : 0
                                    : 0)
                            };

            return vm;
        }

        [HttpPost]
        public JsonResult Save(string band)
        {
            var b = JsonConvert.DeserializeObject<BandDetail>(band);
            IEnumerable<string> msgs;
            var bandId = b.Id;

            if (bandId > 0)
            {
                msgs = ValidateBand(b.Name, false);
                if (msgs == null)
                    UpdateBand(b);

                ReloadUserBands(bandId, false);
            }
            else
            {
                msgs = ValidateBand(b.Name, true);
                if (msgs == null)
                    bandId = AddBand(b);

                ReloadUserBands(bandId, true);
            }

            return Json(new
            {
                BandList = GetBandList(),
                DefaultSingerArrayList = _bandRepository.GetDefaultSingerNameArrayList(),
                SelectedId = bandId,
                Success = (null == msgs),
                ErrorMessages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<string> ValidateBand(string name, bool addNew)
        {
            return _validationRules.ValidateBand(name, addNew);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            _songRepository.DeleteBandSetlistSongs(id);
            _songRepository.DeleteBandSongs(id);
            _userRepository.DeleteUserPreferenceTableColumns(id);
            _userRepository.DeleteUserPreferenceTableMembers(id);
            _userRepository.DeleteUserBands(id);
            _userRepository.UpdateDefaultBandIdAllUsers(id);

            if (_currentUser.DefaultBand != null)
            {
                if (_currentUser.DefaultBand.Id == id)
                {
                    var user = _userRepository.Get(_currentUser.Id);
                    user.DefaultBand = null;
                    _userRepository.Update(user);
                }
            }

            _bandRepository.Delete(id);

            ReloadUserBands(id, false, true);

            return Json(new
            {
                BandList = GetBandList(),
                Success = true,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveColumns(string columns)
        {
            _common.SaveColumns(columns, Constants.UserTable.BandId);
            return Json(JsonRequestBehavior.AllowGet);
        }

        private int AddBand(BandDetail bandDetail)
        {
            var b = new Band
            {
                Name = bandDetail.Name,
                UserCreate = _currentUser,
                UserUpdate = _currentUser,
                DateCreate = DateTime.Now,
                DateUpdate = DateTime.Now
            };

            var id = _bandRepository.Add(b);
            if (id <= 0) return id;

            _userRepository.AddUserBand(_currentUser.Id, id);
            _userRepository.AddUserPreferenceTableColumns(_currentUser.Id, id);

            return id;
        }

        private void ReloadUserBands(int bandId, bool isAdded, bool isDeleted = false)
        {
            var userBands = _currentUser.UserBands;

            Session["Bands"] = null;            

            if (userBands.Any())
            {
                Session["Bands"] = userBands.Select(x => new
                {
                    x.Band.Id, x.Band.Name
                }).ToArray();
            }

            if (isDeleted)
            {
                if ((int) Session["BandId"] == bandId)
                {
                    Session["BandId"] = userBands.First().Band.Id;
                }
            }
            else if (isAdded)
            {
                Session["BandId"] = bandId;
            }
        }

        private void UpdateBand(BandDetail bandDetail)
        {
            var band = _bandRepository.Get(bandDetail.Id);

            Member defaultSinger = null;
            if (bandDetail.DefaultSingerId > 0)
            {
                defaultSinger = _memberRepository.Get(bandDetail.DefaultSingerId);
            }

            Genre defaultGenre = null;
            if (bandDetail.DefaultGenreId > 0)
            {
                defaultGenre = _bandRepository.GetGenre(bandDetail.DefaultGenreId);
            }

            if (band != null)
            {
                band.Name = bandDetail.Name;
                band.DefaultSinger = defaultSinger;
                band.DefaultGenre = defaultGenre;
                band.UserUpdate = _currentUser;
                band.DateUpdate = DateTime.Now;
            };

            _bandRepository.Update(band);
        }
    }
}
