using System.Web.Mvc;

namespace SetGenerator.WebUI.Controllers
{
    public class BandsController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

    }
}
