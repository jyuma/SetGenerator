using SetGenerator.WebUI.ViewModels;
using SetGenerator.Domain.Entities;
using System.Web.Mvc;

namespace SetGenerator.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly User _currentUser;

        public ActionResult Index(LogonViewModel model)
        {
            return View(LoadLogonViewModel());
        }

        public string GetCurrentSessionUser()
        {
            return System.Web.HttpContext.Current.User.Identity.Name;
        }

        private LogonViewModel LoadLogonViewModel()
        {
            var model = new LogonViewModel
            {
                UserName = GetCurrentSessionUser()
            };


            return model;
        }

        [HttpPost]
        public ActionResult SetCurrentBand(int bandId)
        {
            Session["BandId"] = bandId;
            return Json(JsonRequestBehavior.AllowGet);
        }
    }
}
