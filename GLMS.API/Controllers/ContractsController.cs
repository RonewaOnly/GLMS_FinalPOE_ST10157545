using Microsoft.AspNetCore.Mvc;

namespace GLMS.API.Controllers
{
    public class ContractsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
