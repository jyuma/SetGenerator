using Newtonsoft.Json;
using SetGenerator.Service;
using SetGenerator.WebUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using NHibernate.Hql.Ast.ANTLR.Tree;
using SetGenerator.Data.Repositories;
using SetGenerator.Domain.Entities;
using SetGenerator.WebUI.Common;

namespace SetGenerator.WebUI.Controllers
{
    [RoutePrefix("Bands")]
    public class BandsController : Controller
    {
        private readonly IBandRepository _bandRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly IValidationRules _validationRules;
        private readonly User _currentUser;
        private readonly CommonSong _common;

        public BandsController(IBandRepository bandRepository,
                                IMemberRepository memberRepository,
                                IInstrumentRepository instrumentRepository,
                                IValidationRules validationRules,
                                IAccount account)
        {
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
                TableColumnList = _common.GetTableColumnList(_currentUser.Id, Constants.UserTable.BandId)
            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<BandDetail> GetBandList()
        {
            var bandList = _bandRepository.GetAll();

            var result = bandList.Select(band => new BandDetail
            {
                Id = band.Id,
                Name = band.Name,
                DefaultSingerId = (band.DefaultSinger != null) ? band.DefaultSinger.Id : 0,
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
                Members =
                    new SelectList((band != null)
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
                            : 0)
            };

            return vm;
        }

        [HttpPost]
        public JsonResult Save(string band)
        {
            var g = JsonConvert.DeserializeObject<BandDetail>(band);
            List<string> msgs;
            var bandId = g.Id;

            if (bandId > 0)
            {
                msgs = ValidateBand(g.Name, false);
                if (msgs == null)
                    UpdateBand(g);
            }
            else
            {
                msgs = ValidateBand(g.Name, true);
                if (msgs == null)
                    bandId = AddBand(g);
            }

            return Json(new
            {
                BandList = GetBandList(),
                SelectedId = bandId,
                Success = (null == msgs),
                ErrorMessages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public List<string> ValidateBand(string name, bool addNew)
        {
            return _validationRules.ValidateBand(name, addNew);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            _bandRepository.Delete(id);

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

            return _bandRepository.Add(b);
        }

        private void UpdateBand(BandDetail bandDetail)
        {
            var band = _bandRepository.Get(bandDetail.Id);

            if (band != null)
            {
                band.Name = bandDetail.Name;
                band.UserUpdate = _currentUser;
                band.DateUpdate = DateTime.Now;
            };

            _bandRepository.Update(band);
        }


        // Members
        [Route("{id}/Members/")]
        [Authorize]
        public ActionResult Members(int id)
        {
            return View(LoadMemberViewModel(id, 0, null));
        }

        [HttpGet]
        public JsonResult GetDataMembers(int bandId)
        {
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
                DefaultInstrumentId = member.DefaultInstrument.Id 
            }).OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName).ToArray();

            return result;
        }

        private MemberViewModel LoadMemberViewModel(int bandId, int selectedId, List<string> msgs)
        {
            var bandName = _bandRepository.Get(bandId).Name;

            var model = new MemberViewModel
            {
                BandName = bandName,
                BandId = bandId,
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
                FirstName = (member != null) ? member.FirstName : string.Empty,
                LastName = (member != null) ? member.LastName : string.Empty,
                Alias = (member != null) ? member.Alias : string.Empty,
                MemberInstruments =
                    new SelectList((member != null)
                    ? new Collection<object>{ new { Value = "0", Display = "<None>" }}.ToArray()
                    .Union(
                        member.MemberInstruments
                        .Select(x => new
                        {
                            Value = x.Instrument.Id,
                            Display = x.Instrument.Name
                        })).ToArray()
                     : new Collection<object>().ToArray(), "Value", "Display",
                        (member != null) 
                            ? (member.DefaultInstrument != null) ? member.DefaultInstrument.Id : 0
                            : 0)

            };

            return vm;
        }

        [HttpPost]
        public JsonResult SaveMember(string member)
        {
            var m = JsonConvert.DeserializeObject<MemberDetail>(member);
            List<string> msgs = null;
            var memberId = m.Id;

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
                SelectedId = memberId,
                Success = (null == msgs),
                ErrorMessages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public List<string> ValidateMember(string firstName, string lastName, string alias, bool addNew)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            return _validationRules.ValidateMember(bandId, firstName, lastName, alias, addNew);
        }

        [HttpPost]
        public JsonResult DeleteMember(int id)
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
        public JsonResult SaveColumnsMembers(string columns)
        {
            _common.SaveColumns(columns, Constants.UserTable.MemberId);
            return Json(JsonRequestBehavior.AllowGet);
        }

        private int AddMember(MemberDetail memberDetail)
        {
            var bandId = Convert.ToInt32(Session["BandId"]);
            var band = _bandRepository.Get(bandId);
            var defaultInstrument = _instrumentRepository.Get(memberDetail.DefaultInstrumentId);

            var m = new Member
            {
                Band = band,
                FirstName = memberDetail.FirstName,
                LastName = memberDetail.LastName,
                Alias = memberDetail.Alias,
                DefaultInstrument = defaultInstrument
            };

            return _memberRepository.Add(m);
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
                SelectedInstruments =
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
    }
}
