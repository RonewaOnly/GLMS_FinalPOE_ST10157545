using GLMS.API.Models.DTOs;
using GLMS.Web.Data;
using GLMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ApiClient _api;
        public ClientsController(ApiClient api) => _api = api;
        public async Task<IActionResult> Index() => View(await _api.GetAsync<List<ClientDto>>("api/clients") ?? new());
        public async Task<IActionResult> Details(int id) { var c = await _api.GetAsync<ClientDto>($"api/clients/{id}"); if (c == null) return NotFound(); return View(c); }
        [HttpGet] public IActionResult Create() => View(new ClientDto());
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientDto vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var (ok, _, err) = await _api.PostAsync<ClientDto>("api/clients", new { vm.Name, vm.ContractDetails, vm.Region });
            if (!ok) { ModelState.AddModelError("", err ?? "Error."); return View(vm); }
            TempData["Success"] = $"Client '{vm.Name}' created.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet] public async Task<IActionResult> Edit(int id) { var c = await _api.GetAsync<ClientDto>($"api/clients/{id}"); if (c == null) return NotFound(); return View(c); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientDto vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var (ok, err) = await _api.PutAsync($"api/clients/{id}", new { vm.Name, vm.ContractDetails, vm.Region });
            if (!ok) { ModelState.AddModelError("", err ?? "Update failed."); return View(vm); }
            TempData["Success"] = "Client updated."; return RedirectToAction(nameof(Index));
        }
        [HttpGet] public async Task<IActionResult> Delete(int id) { var c = await _api.GetAsync<ClientDto>($"api/clients/{id}"); if (c == null) return NotFound(); return View(c); }
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) { var ok = await _api.DeleteAsync($"api/clients/{id}"); TempData[ok ? "Success" : "Error"] = ok ? "Client deleted." : "Cannot delete — client has contracts."; return RedirectToAction(nameof(Index)); }

    }
}
