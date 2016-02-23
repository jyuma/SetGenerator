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

namespace SetGenerator.WebUI.Controllers
{
    [RoutePrefix("Users")]
    public class UsersController : Controller
    {
        private readonly IBandRepository _bandRepository;
        private readonly IUserRepository _userRepository;
        private readonly IValidationRules _validationRules;
        private readonly User _currentUser;
        private readonly CommonSong _common;

        public UsersController( IBandRepository bandRepository,
                                IUserRepository userRepository,
                                IValidationRules validationRules,
                                IAccount account)
        {
            _bandRepository = bandRepository;
            _userRepository = userRepository;
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
                TableColumnList = _common.GetTableColumnList(_currentUser.Id, Constants.UserTable.UserId)
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
            var b = JsonConvert.DeserializeObject<UserDetail>(user);
            List<string> msgs;
            var userId = b.Id;

            if (userId > 0)
            {
                msgs = ValidateUser(b.UserName, false);
                if (msgs == null)
                    UpdateUser(b);
            }
            else
            {
                msgs = ValidateUser(b.UserName, true);
                if (msgs == null)
                    userId = AddUser(b);
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

        [HttpGet]
        public List<string> ValidateUser(string username, bool addNew)
        {
            return _validationRules.ValidateBand(username, addNew);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            _userRepository.DeleteUserPreferenceTableColumns(_currentUser.Id, id);
            _userRepository.DeleteUserPreferenceTableMembers(_currentUser.Id, id);
            _userRepository.DeleteUserBand(_currentUser.Id, id);
            _userRepository.Delete(id);

            return Json(new
            {
                UserList = GetUserList(),
                Success = true,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveColumns(string columns)
        {
            _common.SaveColumns(columns, Constants.UserTable.UserId);
            return Json(JsonRequestBehavior.AllowGet);
        }

        private int AddUser(UserDetail userDetail)
        {            
            var u = new User
            {
                UserName = userDetail.UserName,
                Email = userDetail.UserName,
                IsDisabled = userDetail.IsDisabled,
            };

            var id = _userRepository.Add(u);
            //if (id <= 0) return id;

            //_userRepository.Add(_currentUser);
            //_userRepository.AddUserPreferenceTableColumns(_currentUser.Id, id);

            return id;
        }

        private void UpdateUser(UserDetail userDetail)
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
            //var user = _userRepository.Get(detail.UserId);
            _userRepository.AddRemoveUserBands(detail.UserId, detail.BandIds);

            return Json(new
            {
                UserList = GetUserList(),
                DefaultBandArrayList = _userRepository.GetDefaultBandArrayList(),
                SelectedId = detail.UserId,
                Success = true,
                ErrorMessages = string.Empty
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
