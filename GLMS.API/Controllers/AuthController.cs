using Microsoft.AspNetCore.Mvc;

namespace GLMS.API.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
