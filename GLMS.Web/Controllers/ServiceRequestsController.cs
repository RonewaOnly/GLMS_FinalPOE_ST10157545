using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web.Controllers
{
    public class ServiceRequestsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
