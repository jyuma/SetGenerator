using Newtonsoft.Json;
using SetGenerator.Domain;
using SetGenerator.Repositories;
using SetGenerator.Service;
using SetGenerator.WebUI.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;

namespace SetGenerator.WebUI.Controllers
{
    public class GigsController : Controller
    {
        private readonly IGigRepository _gigRepository;
        private readonly IAccount _account;
        private readonly int _bandId = 1;
        private readonly string _currentUserName;
        private readonly User _currentUser;

        public GigsController(IGigRepository gigRepository, IAccount account)
        {
            _gigRepository = gigRepository;
            _account = account;
            _currentUserName = GetCurrentSessionUser();
            if (_currentUserName.Length > 0)
                _currentUser = _account.GetUserByUserName(_currentUserName);
        }

        [Authorize]
        public ActionResult Index()
        {
            var gigs = _gigRepository.GetList(_bandId);
            return View(LoadGigViewModel(gigs, 0, null));
        }

        public string GetCurrentSessionUser()
        {
            return (System.Web.HttpContext.Current.User.Identity.Name);
        }

        private GigViewModel LoadGigViewModel(IEnumerable<Gig> gigs, int selectedId, List<string> msgs)
        {
            var model = new GigViewModel();
            var user = _account.GetUserByUserName(_currentUserName);
            var list = gigs.Select(g => new GigDetail
            {
                Id = g.Id,
                BandId = g.BandId,
                DateGig = g.DateGig.ToShortDateString(),
                Venue = g.Venue,
                Description = g.Description,
                UserUpdate = g.User1.UserName,
                DateUpdate = g.DateUpdate.ToShortDateString()
            }).ToList();
            model.SelectedId = selectedId;
            model.GigList = list;
            model.TableColumnList = GetTableColumnList(user);
            model.Success = (msgs == null);
            model.ErrorMessages = msgs;
            return model;
        }

        // -- for the table columns
        private static IList<TableColumnDetail> GetTableColumnList(User u)
        {
            var list = new List<TableColumnDetail>();

            if (u != null)
            {
                list.AddRange(u.UserPreferenceTableColumns.Where(x => x.TableColumn.TableId == Constants.UserTable.GigId).Select(tc => new TableColumnDetail
                {
                    Header = tc.TableColumn.Name,
                    Data = tc.TableColumn.Data,
                    IsVisible = tc.IsVisible,
                    AlwaysVisible = tc.TableColumn.AlwaysVisible,
                    IsMember = false
                }));

                list.AddRange(u.UserPreferenceTableMembers.Where(x => x.TableId == Constants.UserTable.GigId).Select(tc => new TableColumnDetail
                {
                    Header = tc.Member.FirstName,
                    Data = tc.Member.FirstName.ToLower(),
                    IsVisible = tc.IsVisible,
                    AlwaysVisible = false,
                    IsMember = true
                }));
            }
            return list;
        }

        [HttpPost]
        public JsonResult Save(string gig)
        {
            var g = JsonConvert.DeserializeObject<GigDetail>(gig);
            List<string> msgs = null;
            var gigId = g.Id;

            if (gigId > 0)
            {
                //  msgs = ValidateGig(s.Title, false);
                if (msgs == null)
                    UpdateGig(g);
            }
            else
            {
                //  msgs = ValidateGig(s.Title, true);
                if (msgs == null)
                    gigId = AddGig(g);
            }

            var songs = _gigRepository.GetList(g.BandId);
            var vm = LoadGigViewModel(songs, gigId, msgs);
            return Json(vm);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            var g = _gigRepository.GetSingle(id);

            DeleteGig(g);
            var songs = _gigRepository.GetList(g.BandId);
            var vm = LoadGigViewModel(songs, id, null);
            return Json(vm);
        }

        public JsonResult SaveColumns(string columns)
        {
            var cList = JsonConvert.DeserializeObject<IList<TableColumnDetail>>(columns);
            var cols = new OrderedDictionary();
            foreach (var c in cList)
                cols.Add(c.Data, c.IsVisible);
            _account.UpdateUserTablePreferences(_currentUserName, Constants.UserTable.GigId, cols);
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        private int AddGig(GigDetail gig)
        {
            var g = new Gig
            {
                DateGig = Convert.ToDateTime(gig.DateGig),
                Venue = gig.Venue,
                Description = gig.Description,
                BandId = gig.BandId,
                UserCreateId = _currentUser.Id,
                UserUpdateId = _currentUser.Id,
                DateCreate = DateTime.Now,
                DateUpdate = DateTime.Now
            };

            return _gigRepository.Add(g);
        }

        private void UpdateGig(GigDetail gd)
        {
            var gig = _gigRepository.GetSingle(gd.Id);
            if (gig != null)
            {
                gig.DateGig = Convert.ToDateTime(gd.DateGig);
                gig.Venue = gd.Venue;
                gig.Description = gd.Description;
                gig.BandId = gd.BandId;
                gig.UserUpdateId = _currentUser.Id;
                gig.DateUpdate = DateTime.Now;
            };
            _gigRepository.Update();
        }

        private void DeleteGig(Gig g)
        {
            _gigRepository.Delete(g);
        }
    }
}
