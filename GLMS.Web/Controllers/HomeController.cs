using GLMS.Shared.Models.DTOs;
using GLMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using GLMS.Web.Services;


namespace GLMS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiClient _api;
        public HomeController(ApiClient api) => _api = api;
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("jwt_token") == null) return RedirectToAction("Login", "Auth");
            return View(await _api.GetAsync<DashboardDto>("api/dashboard") ?? new DashboardDto());
        }
        public IActionResult Error() => View();
    }
}
