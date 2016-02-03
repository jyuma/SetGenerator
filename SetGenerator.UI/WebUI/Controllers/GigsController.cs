using Newtonsoft.Json;
using SetGenerator.Service;
using SetGenerator.WebUI.ViewModels;
using System;
using System.Collections.Generic;
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
        private readonly IValidationRules _validationRules;
        private readonly User _currentUser;
        private readonly CommonSong _common;

        public GigsController(  IBandRepository bandRepository,
                                IGigRepository gigRepository,
                                IValidationRules validationRules, 
                                IAccount account)
        {
            _bandRepository = bandRepository;
            _gigRepository = gigRepository;
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
                GigList = GetGigList(),
                TableColumnList = _common.GetTableColumnList(
                    _currentUser.UserPreferenceTableColumns,
                    _currentUser.UserPreferenceTableMembers.Where(x => x.Member.Band.Id == bandId),
                    Constants.UserTable.GigId)
            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<GigDetail> GetGigList()
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var gigList = _gigRepository.GetByBandId(bandId);

            var result = gigList.Select(gig => new GigDetail
            {
                Id = gig.Id,
                BandId = gig.Band.Id,
                Venue = gig.Venue,
                Description = gig.Description,
                DateGig = gig.DateGig.ToShortDateString(),
                UserUpdate = gig.UserUpdate.UserName,
                DateUpdate = gig.DateUpdate.ToShortDateString()
            }).OrderBy(x => x.DateGig).ToArray();

            return result;
        }

        public string GetCurrentSessionUser()
        {
            return (System.Web.HttpContext.Current.User.Identity.Name);
        }

        private static GigViewModel LoadGigViewModel(int selectedId, List<string> msgs)
        {
            var model = new GigViewModel
            {
                SelectedId = selectedId,
                Success = (msgs == null),
                ErrorMessages = msgs
            };

            return model;
        }

        [HttpGet]
        public PartialViewResult GetGigEditView(int id)
        {
            return PartialView("_GigEdit", LoadSetlistEditViewModel(id));
        }

        private GigEditViewModel LoadSetlistEditViewModel(int id)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
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
                Venues = _gigRepository.GetVenueList(bandId)
            };

            return vm;
        }

        [HttpPost]
        public JsonResult Save(string gig)
        {
            var g = JsonConvert.DeserializeObject<GigDetail>(gig);
            List<string> msgs = null;
            var gigId = g.Id;

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
                GigList = GetGigList(),
                SelectedId = gigId,
                Success = (null == msgs),
                ErrorMessages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public List<string> ValidateGig(string venue, string dateGig, bool addNew)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            return _validationRules.ValidateGig(bandId, venue, Convert.ToDateTime(dateGig), addNew);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            //var g = _gigRepository.Get(id);

            _gigRepository.Delete(id);

            return Json(new
            {
                GigList = GetGigList(),
                Success = true,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveColumns(string columns)
        {
            _common.SaveColumns(columns, Constants.UserTable.GigId);
            return Json(JsonRequestBehavior.AllowGet);
        }

        //// --- for the autocomplete
        //[HttpGet]
        //public ActionResult GetVenueList()
        //{
        //    var bandId = Convert.ToInt32(Session["BandId"]);
        //    var list = _gigRepository.GetVenueList(bandId);
        //    return Json(list, JsonRequestBehavior.AllowGet);
        //}

        private int AddGig(GigDetail gigDetail)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var band = _bandRepository.Get(bandId);

            var g = new Gig
            {
                Band = band,
                DateGig = Convert.ToDateTime(gigDetail.DateGig),
                Venue = gigDetail.Venue,
                Description = gigDetail.Description,
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

            if (gig != null)
            {
                gig.DateGig = Convert.ToDateTime(gigDetail.DateGig);
                gig.Venue = gigDetail.Venue;
                gig.Description = gigDetail.Description;
                gig.UserUpdate = _currentUser;
                gig.DateUpdate = DateTime.Now;
            };

            _gigRepository.Update(gig);
        }
    }
}
