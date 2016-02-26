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
    public class GigsController : Controller
    {
        private readonly IBandRepository _bandRepository;
        private readonly IGigRepository _gigRepository;
        private readonly ISetlistRepository _setlistRepository;
        private readonly IValidationRules _validationRules;
        private readonly User _currentUser;
        private readonly CommonSong _common;

        public GigsController(  IBandRepository bandRepository,
                                IGigRepository gigRepository,
                                ISetlistRepository setlistRepository,
                                IValidationRules validationRules, 
                                IAccount account)
        {
            _bandRepository = bandRepository;
            _gigRepository = gigRepository;
            _setlistRepository = setlistRepository;
            _validationRules = validationRules;

            var currentUserName = GetCurrentSessionUser();
            if (currentUserName.Length > 0)
                _currentUser = account.GetUserByUserName(currentUserName);
            _common = new CommonSong(account, currentUserName);
        }

        [Authorize]
        public ActionResult Index()
        {
            return View(LoadGigViewModel(0, null));
        }

        [HttpGet]
        public JsonResult GetData()
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var vm = new
            {
                GigList = GetGigList(bandId),
                SetlistArrayList = _setlistRepository.GetSetlistArrayList(bandId),
                TableColumnList = _common.GetTableColumnList(_currentUser.Id, Constants.UserTable.GigId, bandId)
            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<GigDetail> GetGigList(int bandId)
        {
            var gigList = _gigRepository.GetByBandId(bandId);

            var result = gigList.Select(gig => new GigDetail
            {
                Id = gig.Id,
                BandId = gig.Band.Id,
                Venue = gig.Venue,
                Description = gig.Description,
                DateGig = gig.DateGig.ToShortDateString(),
                SetlistId = (gig.Setlist != null) ? gig.Setlist.Id : 0,
                UserUpdate = gig.UserUpdate.UserName,
                DateUpdate = gig.DateUpdate.ToShortDateString()
            }).OrderBy(x => x.DateGig).ToArray();

            return result;
        }

        public string GetCurrentSessionUser()
        {
            return (System.Web.HttpContext.Current.User.Identity.Name);
        }

        private GigViewModel LoadGigViewModel(int selectedId, List<string> msgs)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var bandName = _bandRepository.Get(bandId).Name;

            var model = new GigViewModel
            {
                BandName = bandName,
                SelectedId = selectedId,
                Success = (msgs == null),
                ErrorMessages = msgs
            };

            return model;
        }

        [HttpGet]
        public PartialViewResult GetGigEditView(int id)
        {
            return PartialView("_GigEdit", LoadGigEditViewModel(id));
        }

        private GigEditViewModel LoadGigEditViewModel(int id)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var setlists = _setlistRepository.GetByBandId(bandId);
            Gig gig = null;

            if (id > 0)
            {
                gig = _gigRepository.Get(id);
            }
            var vm = new GigEditViewModel
            {
                DateGig = (gig != null) ? gig.DateGig : DateTime.Now,
                Venue = (gig != null) ? gig.Venue : string.Empty,
                Description = (gig != null) ? gig.Description : string.Empty,
                Venues = _gigRepository.GetVenueList(bandId),
                Setlists = new SelectList(
                           new Collection<object> { new { Value = "0", Display = "<None>" } }.ToArray()
                           .Union(
                               setlists
                               .Select(x => new
                               {
                                   Value = x.Id,
                                   Display = x.Name
                               })).ToArray(), "Value", "Display",
                               (gig != null)
                                   ? (gig.Setlist != null) ? gig.Setlist.Id : 0
                                   : 0),
            };

            return vm;
        }

        [HttpPost]
        public JsonResult Save(string gig)
        {
            var g = JsonConvert.DeserializeObject<GigDetail>(gig);
            IEnumerable<string> msgs;
            var gigId = g.Id;
            g.BandId = Convert.ToInt32(Session["BandId"]);

            if (gigId > 0)
            {
                msgs = ValidateGig(g.Venue, g.DateGig, false);
                if (msgs == null)
                    UpdateGig(g);
            }
            else
            {
                msgs = ValidateGig(g.Venue, g.DateGig, true);
                if (msgs == null)
                    gigId = AddGig(g);
            }

            return Json(new
            {
                GigList = GetGigList(g.BandId),
                SelectedId = gigId,
                Success = (null == msgs),
                ErrorMessages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<string> ValidateGig(string venue, string dateGig, bool addNew)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            return _validationRules.ValidateGig(bandId, venue, Convert.ToDateTime(dateGig), addNew);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            _gigRepository.Delete(id);
            var bandId = Convert.ToInt32(Session["BandId"]);

            return Json(new
            {
                GigList = GetGigList(bandId),
                Success = true,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveColumns(string columns)
        {
            _common.SaveColumns(columns, Constants.UserTable.GigId);
            return Json(JsonRequestBehavior.AllowGet);
        }

        private int AddGig(GigDetail gigDetail)
        {
            var band = _bandRepository.Get(gigDetail.BandId);
            var setlist = _setlistRepository.Get(gigDetail.SetlistId);

            var g = new Gig
            {
                Band = band,
                DateGig = Convert.ToDateTime(gigDetail.DateGig),
                Venue = gigDetail.Venue,
                Description = gigDetail.Description,
                Setlist = setlist,
                UserCreate = _currentUser,
                UserUpdate = _currentUser,
                DateCreate = DateTime.Now,
                DateUpdate = DateTime.Now
            };

            return _gigRepository.Add(g);
        }

        private void UpdateGig(GigDetail gigDetail)
        {
            var gig = _gigRepository.Get(gigDetail.Id);
            var setlist = _setlistRepository.Get(gigDetail.SetlistId);

            if (gig != null)
            {
                gig.DateGig = Convert.ToDateTime(gigDetail.DateGig);
                gig.Venue = gigDetail.Venue;
                gig.Description = gigDetail.Description;
                gig.Setlist = setlist;
                gig.UserUpdate = _currentUser;
                gig.DateUpdate = DateTime.Now;
            };

            _gigRepository.Update(gig);
        }
    }
}
