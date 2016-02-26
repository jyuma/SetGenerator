using SetGenerator.Domain.Entities;
using SetGenerator.Service;
using SetGenerator.WebUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using SetGenerator.Data.Repositories;
using SetGenerator.WebUI.Common;

namespace SetGenerator.WebUI.Controllers
{
    public class MembersController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IBandRepository _bandRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly IValidationRules _validationRules;
        private readonly User _currentUser;
        private readonly CommonSong _common;

        public MembersController(IUserRepository userRepository,
                                IBandRepository bandRepository,
                                IMemberRepository memberRepository,
                                IInstrumentRepository instrumentRepository,
                                IValidationRules validationRules,
                                IAccount account)
        {
            _userRepository = userRepository;
            _bandRepository = bandRepository;
            _memberRepository = memberRepository;
            _instrumentRepository = instrumentRepository;
            _validationRules = validationRules;

            var currentUserName = GetCurrentSessionUser();
            if (currentUserName.Length > 0)
                _currentUser = account.GetUserByUserName(currentUserName);
            _common = new CommonSong(account, currentUserName);
        }

        [Authorize]
        public ActionResult Index()
        {
            return View(LoadMemberViewModel(0, null));
        }

        [HttpGet]
        public JsonResult GetData()
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var vm = new
            {
                MemberList = GetMemberList(bandId),
                InstrumentArrayList = _instrumentRepository.GetNameArrayList(),
                TableColumnList = _common.GetTableColumnList(_currentUser.Id, Constants.UserTable.MemberId, bandId)
            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<MemberDetail> GetMemberList(int bandId)
        {
            var memberList = _memberRepository.GetByBandId(bandId);

            var result = memberList.Select(member => new MemberDetail
            {
                Id = member.Id,
                BandId = member.Band.Id,
                FirstName = member.FirstName,
                LastName = member.LastName,
                Alias = member.Alias,
                DefaultInstrumentId = (member.DefaultInstrument != null) ? member.DefaultInstrument.Id : 0,
            }).OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName).ToArray();

            return result;
        }

        public string GetCurrentSessionUser()
        {
            return (System.Web.HttpContext.Current.User.Identity.Name);
        }

        private MemberViewModel LoadMemberViewModel(int selectedId, List<string> msgs)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var bandName = _bandRepository.Get(bandId).Name;

            var model = new MemberViewModel
            {
                BandName = bandName,
                SelectedId = selectedId,
                Success = (msgs == null),
                ErrorMessages = msgs
            };

            return model;
        }

        [HttpGet]
        public PartialViewResult GetMemberEditView(int id)
        {
            return PartialView("_MemberEdit", LoadMemberEditViewModel(id));
        }

        private MemberEditViewModel LoadMemberEditViewModel(int id)
        {
            Member member = null;

            if (id > 0)
            {
                member = _memberRepository.Get(id);
            }
            var vm = new MemberEditViewModel
            {
                Id = (member != null) ? member.Id : 0,
                FirstName = (member != null) ? member.FirstName : string.Empty,
                LastName = (member != null) ? member.LastName : string.Empty,
                Alias = (member != null) ? member.Alias : string.Empty,
                MemberInstruments =
                    new SelectList((member != null)
                    ? new Collection<object> { new { Value = "0", Display = "<None>" } }.ToArray()
                    .Union(
                        member.MemberInstruments
                        .OrderBy(o => o.Instrument.Name)
                        .Select(x => new
                        {
                            Value = x.Instrument.Id,
                            Display = x.Instrument.Name
                        })).ToArray()
                     : new Collection<object> { new { Value = "0", Display = "<None>" } }.ToArray()
                     .Union(
                     _instrumentRepository.GetAll()
                        .OrderBy(o => o.Name)
                        .Select(x => new
                        {
                            Value = x.Id,
                            Display = x.Name
                        })).ToArray(),
                     "Value", "Display",
                        (member != null)
                            ? (member.DefaultInstrument != null) ? member.DefaultInstrument.Id : 0
                            : 0)

            };

            return vm;
        }

        [HttpPost]
        public JsonResult Save(string member)
        {
            var m = JsonConvert.DeserializeObject<MemberDetail>(member);
            var memberId = m.Id;
            m.BandId = Convert.ToInt32(Session["BandId"]);

            IEnumerable<string> msgs;

            if (memberId > 0)
            {
                msgs = ValidateMember(m.FirstName, m.LastName, m.Alias, false);
                if (msgs == null)
                    UpdateMember(m);
            }
            else
            {
                msgs = ValidateMember(m.FirstName, m.LastName, m.Alias, true);
                if (msgs == null)
                    memberId = AddMember(m);
            }

            return Json(new
            {
                MemberList = GetMemberList(m.BandId),
                InstrumentArrayList = _instrumentRepository.GetNameArrayList(),
                SelectedId = memberId,
                Success = (null == msgs),
                ErrorMessages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<string> ValidateMember(string firstName, string lastName, string alias, bool addNew)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            return _validationRules.ValidateMember(bandId, firstName, lastName, alias, addNew);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            var bandId = _memberRepository.Get(id).Band.Id;
            _memberRepository.Delete(id);

            return Json(new
            {
                MemberList = GetMemberList(bandId),
                Success = true,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveColumns(string columns)
        {
            _common.SaveColumns(columns, Constants.UserTable.MemberId);
            return Json(JsonRequestBehavior.AllowGet);
        }

        private int AddMember(MemberDetail memberDetail)
        {
            var band = _bandRepository.Get(memberDetail.BandId);
            var defaultInstrument = _instrumentRepository.Get(memberDetail.DefaultInstrumentId);

            var m = new Member
            {
                Band = band,
                FirstName = memberDetail.FirstName,
                LastName = memberDetail.LastName,
                Alias = memberDetail.Alias,
                DefaultInstrument = defaultInstrument
            };

            if (defaultInstrument != null)
            {
                m.MemberInstruments = new Collection<MemberInstrument>
                {
                    new MemberInstrument
                    {
                        Member = m,
                        Instrument = defaultInstrument
                    }
                };
            }

            var id = _memberRepository.Add(m);

            _userRepository.AddAllUserPreferenceTableMember(memberDetail.BandId, id);

            return id;
        }

        private void UpdateMember(MemberDetail memberDetail)
        {
            var member = _memberRepository.Get(memberDetail.Id);
            var defaultInstrument = _instrumentRepository.Get(memberDetail.DefaultInstrumentId);

            if (member != null)
            {
                member.FirstName = memberDetail.FirstName;
                member.LastName = memberDetail.LastName;
                member.Alias = memberDetail.Alias;
                member.DefaultInstrument = defaultInstrument;
            };

            _memberRepository.Update(member);
        }

        // Member Instruments

        [HttpGet]
        public PartialViewResult GetMemberInstrumentEditView(int id)
        {
            return PartialView("_MemberInstrumentEdit", LoadMemberInstrumentEditViewModel(id));
        }

        private MemberInstrumentEditViewModel LoadMemberInstrumentEditViewModel(int memberId)
        {
            var memberInstruments = _memberRepository.GetInstruments(memberId)
                .Select(x => new { x.Id, x.Name })
                .ToArray();

            var allInstruments = _instrumentRepository.GetAll()
                .OrderBy(o => o.Name)
                .Select(x => new { x.Id, x.Name });

            var vm = new MemberInstrumentEditViewModel
            {
                AssignedInstruments =
                    new SelectList(
                        memberInstruments
                        .Select(x => new
                        {
                            Value = x.Id,
                            Display = x.Name
                        }).ToArray(), "Value", "Display"),

                AvailableInstruments = new SelectList(
                        allInstruments
                        .Where(x => !memberInstruments.Contains(x))
                        .Select(x => new
                        {
                            Value = x.Id,
                            Display = x.Name
                        }).ToArray(), "Value", "Display")

            };

            return vm;
        }

        [HttpPost]
        public JsonResult SaveMemberInstruments(string memberInstrumentDetail)
        {
            var detail = JsonConvert.DeserializeObject<MemberInstrumentDetail>(memberInstrumentDetail);

            _memberRepository.AddRemoveMemberInstruments(detail.MemberId, detail.InstrumentIds);

            var bandId = Convert.ToInt32(Session["BandId"]);
            return Json(new
            {
                MemberList = GetMemberList(bandId),
                InstrumentArrayList = _instrumentRepository.GetNameArrayList(),
                SelectedId = detail.MemberId,
                Success = true,
                ErrorMessages = string.Empty
            }, JsonRequestBehavior.AllowGet);
        }
    }
}