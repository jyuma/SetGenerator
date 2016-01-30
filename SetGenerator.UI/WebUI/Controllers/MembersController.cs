using System.Web.Mvc;

namespace SetGenerator.WebUI.Controllers
{
    public class MembersController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

    }
}
