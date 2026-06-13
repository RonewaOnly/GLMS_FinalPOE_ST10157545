using Microsoft.AspNetCore.Mvc;

namespace GLMS.API.Controllers
{
    public class ClientsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
