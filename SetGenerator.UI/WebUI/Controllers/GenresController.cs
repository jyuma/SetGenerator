using Newtonsoft.Json;
using SetGenerator.Service;
using SetGenerator.WebUI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SetGenerator.Data.Repositories;
using SetGenerator.Domain.Entities;
using SetGenerator.WebUI.Common;

namespace SetGenerator.WebUI.Controllers
{
    [RoutePrefix("Genres")]
    public class GenresController : Controller
    {
        private readonly IGenreRepository _genreRepository;
        private readonly ISongRepository _songRepository;
        private readonly IValidationRules _validationRules;
        private readonly User _currentUser;
        private readonly CommonSong _common;

        public GenresController (   IGenreRepository genreRepository,
                                    ISongRepository songRepository,
                                    IValidationRules validationRules,
                                    IAccount account)
        {
            _genreRepository = genreRepository;
            _songRepository = songRepository;
            _validationRules = validationRules;

            var currentUserName = GetCurrentSessionUser();
            if (currentUserName.Length > 0)
                _currentUser = account.GetUserByUserName(currentUserName);
            _common = new CommonSong(account, currentUserName);
        }

        [Authorize]
        public ActionResult Index(int? id)
        {
            return View(LoadGenreViewModel(0, null));
        }

        [HttpGet]
        public JsonResult GetData()
        {
            var vm = new
            {
                GenreList = GetGenreList(),
                TableColumnList = _common.GetTableColumnList(_currentUser.Id, Constants.UserTable.GenreId)
            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<GenreDetail> GetGenreList()
        {
            var genreList = _genreRepository.GetAll();
            var songGenreIds = _songRepository.GetSongGenreIds();

            var result = genreList
                .GroupJoin(songGenreIds, genre => genre.Id, songGenreId => songGenreId,
                (i, sg) => new GenreDetail
                {
                    Id = i.Id,
                    Name = i.Name,
                    IsSongGenre = sg.Any()
                }).OrderBy(x => x.Name).ToArray();

            return result;
        }

        public string GetCurrentSessionUser()
        {
            return (System.Web.HttpContext.Current.User.Identity.Name);
        }

        private static GenreViewModel LoadGenreViewModel(int selectedId, List<string> msgs)
        {
            var model = new GenreViewModel
            {
                SelectedId = selectedId,
                Success = (msgs == null),
                ErrorMessages = msgs
            };

            return model;
        }

        [HttpGet]
        public PartialViewResult GetGenreEditView(int id)
        {
            return PartialView("_GenreEdit", LoadGenreEditViewModel(id));
        }

        private GenreEditViewModel LoadGenreEditViewModel(int id)
        {
            Genre genre = null;

            if (id > 0)
            {
                genre = _genreRepository.Get(id);
            }
            var vm = new GenreEditViewModel
            {
                Name = (genre != null) ? genre.Name : string.Empty
            };

            return vm;
        }

        [HttpPost]
        public JsonResult Save(string genre)
        {
            var i = JsonConvert.DeserializeObject<GenreDetail>(genre);
            IEnumerable<string> msgs;
            var genreId = i.Id;

            if (genreId > 0)
            {
                msgs = ValidateGenre(i.Name, false);
                if (msgs == null)
                    UpdateGenre(i);
            }
            else
            {
                msgs = ValidateGenre(i.Name, true);
                if (msgs == null)
                    genreId = AddGenre(i);
            }

            return Json(new
            {
                GenreList = GetGenreList(),
                SelectedId = genreId,
                Success = (null == msgs),
                ErrorMessages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<string> ValidateGenre(string name, bool addNew)
        {
            return _validationRules.ValidateGenre(name, addNew);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            _genreRepository.Delete(id);

            return Json(new
            {
                GenreList = GetGenreList(),
                Success = true,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveColumns(string columns)
        {
            _common.SaveColumns(columns, Constants.UserTable.GenreId);
            return Json(JsonRequestBehavior.AllowGet);
        }

        private int AddGenre(GenreDetail genreDetail)
        {
            var i = new Genre
            {
                Name = genreDetail.Name
            };

            return _genreRepository.Add(i);
        }

        private void UpdateGenre(GenreDetail genreDetail)
        {
            var genre = _genreRepository.Get(genreDetail.Id);

            if (genre != null)
            {
                genre.Name = genreDetail.Name;
            };

            _genreRepository.Update(genre);
        }
    }
}
