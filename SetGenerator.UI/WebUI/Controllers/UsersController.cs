using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using SetGenerator.Service;
using SetGenerator.WebUI.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Mvc;
using SetGenerator.Data.Repositories;
using SetGenerator.Domain.Entities;
using SetGenerator.WebUI.Common;
using SetGenerator.WebUI.Models;

namespace SetGenerator.WebUI.Controllers
{
    [RoutePrefix("Users")]
    public class UsersController : Controller
    {
        private readonly IAccount _account;
        private readonly IBandRepository _bandRepository;
        private readonly IUserRepository _userRepository;
        private readonly IValidationRules _validationRules;
        private readonly User _currentUser;
        private readonly CommonSong _common;

        public UsersController( IAccount account,
                                IBandRepository bandRepository,
                                IUserRepository userRepository,
                                IValidationRules validationRules)
        {
            _account = account;
            _bandRepository = bandRepository;
            _userRepository = userRepository;
            _validationRules = validationRules;

            var currentUserName = GetCurrentSessionUser();
            if (currentUserName.Length > 0)
                _currentUser = account.GetUserByUserName(currentUserName);
            _common = new CommonSong(account, currentUserName);
        }


        public UsersController(UserManager<MyUser, long> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<MyUser, long> UserManager { get; private set; }

        [Authorize]
        public ActionResult Index(int? id)
        {
            return View(id != null
                ? LoadUserViewModel(((int)id), null)
                : LoadUserViewModel(0, null));
        }

        [HttpGet]
        public JsonResult GetData()
        {
            var vm = new
            {
                UserList = GetUserList(),
                DefaultBandArrayList = _userRepository.GetDefaultBandArrayList(),
                TableColumnList = _common.GetTableColumnList(_currentUser.Id, Service.Constants.UserTable.UserId)
            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<UserDetail> GetUserList()
        {
            var userList = _userRepository.GetAll();

            var result = userList.Select(user => new UserDetail
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                BrowserInfo = user.BrowserInfo,
                IPAddress = user.IPAddress,
                DateRegistered = user.DateRegistered,
                IsDisabled = user.IsDisabled,
                DefaultBandId = (user.DefaultBand != null) ? user.DefaultBand.Id : 0
            }).OrderBy(x => x.UserName).ToArray();

            return result;
        }

        public string GetCurrentSessionUser()
        {
            return (System.Web.HttpContext.Current.User.Identity.Name);
        }

        private static UserViewModel LoadUserViewModel(int selectedId, List<string> msgs)
        {
            var model = new UserViewModel
            {

                SelectedId = selectedId,
                Success = (msgs == null),
                ErrorMessages = msgs
            };

            return model;
        }

        [HttpGet]
        public PartialViewResult GetUserEditView(int id)
        {
            return PartialView("_UserEdit", LoadUserEditViewModel(id));
        }

        private UserEditViewModel LoadUserEditViewModel(int id)
        {
            User user = null;

            if (id > 0)
            {
                user = _userRepository.Get(id);
            }
            var vm = new UserEditViewModel
            {
                Id = (user != null) ? user.Id : 0,
                UserName = (user != null) ? user.UserName : string.Empty,
                Email = (user != null) ? user.Email : string.Empty,
                UserBands = new SelectList((user != null)
                            ? new Collection<object> { new { Value = "0", Display = "<None>" } }.ToArray()
                            .Union(
                                user.UserBands
                                .Select(x => new
                                {
                                    Value = x.Band.Id,
                                    Display = x.Band.Name
                                })).ToArray()
                             : new Collection<object>().ToArray(), "Value", "Display",
                                (user != null)
                                    ? (user.DefaultBand != null) ? user.DefaultBand.Id : 0
                                    : 0)
            };

            return vm;
        }

        [HttpPost]
        public JsonResult Save(string user)
        {
            var u = JsonConvert.DeserializeObject<UserEditViewModel>(user);
            IEnumerable<string> msgs;
            var userId = 0;

            if (u.Id > 0)
            {
                userId = u.Id;
                msgs = ValidateUser(u.UserName, u.Password, u.Email, false);
                if (msgs == null)
                    UpdateUser(u);
            }
            else
            {
                msgs = ValidateUser(u.UserName, u.Password, u.Email, true);
                if (msgs == null)
                    userId = AddUser(u);
            }

            return Json(new
            {
                UserList = GetUserList(),
                DefaultSingerArrayList = _userRepository.GetDefaultBandArrayList(),
                SelectedId = userId,
                Success = (null == msgs),
                ErrorMessages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<string> ValidateUser(string username, string password, string email, bool addNew)
        {
            return _validationRules.ValidateUser(username, password, email, addNew);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            _account.DeleteUser(id);

            return Json(new
            {
                UserList = GetUserList(),
                Success = true,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveColumns(string columns)
        {
            _common.SaveColumns(columns, Service.Constants.UserTable.UserId);
            return Json(JsonRequestBehavior.AllowGet);
        }

        private int AddUser(UserEditViewModel userDetail)
        {
            var user = new MyUser()
            {
                UserName = userDetail.UserName,
                IPAddress = Request.ServerVariables["REMOTE_ADDR"],
                BrowserInfo = Request.Browser.Type,
                Email = userDetail.Email,
                DateRegistered = DateTime.Now
            };

            using (var userManager = new UserManager<MyUser, long>(
                new UserStore<MyUser, MyRole, long, MyLogin, MyUserRole, MyClaim>(new ApplicationDbContext())))
            {
                var result = userManager.Create(user, userDetail.Password);
                if (result == IdentityResult.Success)
                {
                    user = userManager.FindByName(userDetail.UserName);
                    _userRepository.AssignStartupUserPreferenceTableColumns((int)user.Id);
                }
            }

            return (int)user.Id;
        }

        private void UpdateUser(UserEditViewModel userDetail)
        {
            var user = _userRepository.Get(userDetail.Id);

            Band defaultBand = null;
            if (userDetail.DefaultBandId > 0)
            {
                defaultBand = _bandRepository.Get(userDetail.DefaultBandId);
            }

            if (user != null)
            {
                user.UserName = userDetail.UserName;
                user.Email = userDetail.Email;
                user.IsDisabled = userDetail.IsDisabled;
                user.DefaultBand = defaultBand;
            };

            _userRepository.Update(user);
        }

        [HttpGet]
        public PartialViewResult GetUserBandEditView(int id)
        {
            return PartialView("_UserBandEdit", LoadUserBandEditViewModel(id));
        }

        private UserBandEditViewModel LoadUserBandEditViewModel(int userId)
        {
            var userBands = _userRepository.GetUserBands(userId)
                .Select(x => new { x.Band.Id, x.Band.Name })
                .ToArray();

            var allBands = _bandRepository.GetAll()
                .OrderBy(o => o.Name)
                .Select(x => new { x.Id, x.Name });

            var vm = new UserBandEditViewModel
            {
                AssignedBands =
                    new SelectList(
                        userBands
                        .Select(x => new
                        {
                            Value = x.Id,
                            Display = x.Name
                        }).ToArray(), "Value", "Display"),

                AvailableBands = new SelectList(
                        allBands
                        .Where(x => !userBands.Contains(x))
                        .Select(x => new
                        {
                            Value = x.Id,
                            Display = x.Name
                        }).ToArray(), "Value", "Display")

            };

            return vm;
        }

        [HttpPost]
        public JsonResult SaveUserBands(string userBandDetail)
        {
            var detail = JsonConvert.DeserializeObject<UserBandDetail>(userBandDetail);

            _userRepository.AddRemoveUserBands(detail.UserId, detail.BandIds);
            ReloadUserBands();

            return Json(new
            {
                UserList = GetUserList(),
                DefaultBandArrayList = _userRepository.GetDefaultBandArrayList(),
                SelectedId = detail.UserId,
                Success = true,
                ErrorMessages = string.Empty
            }, JsonRequestBehavior.AllowGet);
        }

        private void ReloadUserBands()
        {
            var userBands = _currentUser.UserBands;

            Session["Bands"] = null;

            if (userBands.Any())
            {
                Session["Bands"] = userBands.Select(x => new
                {
                    x.Band.Id,
                    x.Band.Name
                }).ToArray();
            }

            if (_currentUser.DefaultBand != null)
            {
                Session["BandId"] = _currentUser.DefaultBand.Id;
            }
            else
            {
                Session["BandId"] = userBands.First().Band.Id;
            }
        }
    }
}
