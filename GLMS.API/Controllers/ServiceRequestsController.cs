using Microsoft.AspNetCore.Mvc;

namespace GLMS.API.Controllers
{
    public class ServiceRequestsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
