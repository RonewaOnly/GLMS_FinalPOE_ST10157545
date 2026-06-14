using GLMS.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApiClient _api;
        public AuthController(ApiClient api) => _api = api;
        [HttpGet] public IActionResult Login() => View();
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            if (!ModelState.IsValid) return View(req);
            var (ok, data, _) = await _api.PostAsync<LoginResponse>("api/auth/login", req);
            if (!ok || data == null) { ModelState.AddModelError("", "Invalid username or password."); return View(req); }
            HttpContext.Session.SetString("jwt_token", data.Token);
            HttpContext.Session.SetString("username", data.Username);
            HttpContext.Session.SetString("role", data.Role);
            TempData["Success"] = $"Welcome back, {data.Username}!";
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("Login"); }
    }
}
