using GLMS.Shared.Models.DTOs;
using GLMS.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using GLMS.Web.Services;
using GLMS.Web.Mapping;

namespace GLMS.Web.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ApiClient _api;
        public ClientsController(ApiClient api) => _api = api;

        public async Task<IActionResult> Index()
        {
            var dtoList = await _api.GetAsync<List<ClientDto>>("api/clients") ?? new();
            var model = dtoList.Select(ClientMapper.ToModel).ToList();
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var dto = await _api.GetAsync<ClientDto>($"api/clients/{id}");
            if (dto == null) return NotFound();
            return View(ClientMapper.ToModel(dto));
        }

        [HttpGet]
        public IActionResult Create() => View(new Client());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var (ok, _, err) = await _api.PostAsync<ClientDto>("api/clients",
                new { vm.Name, vm.ContractDetails, vm.Region });

            if (!ok)
            {
                ModelState.AddModelError("", err ?? "Error.");
                return View(vm);
            }

            TempData["Success"] = $"Client '{vm.Name}' created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _api.GetAsync<ClientDto>($"api/clients/{id}");
            if (dto == null) return NotFound();
            return View(ClientMapper.ToModel(dto));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var (ok, err) = await _api.PutAsync($"api/clients/{id}",
                new { vm.Name, vm.ContractDetails, vm.Region });

            if (!ok)
            {
                ModelState.AddModelError("", err ?? "Update failed.");
                return View(vm);
            }

            TempData["Success"] = "Client updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var dto = await _api.GetAsync<ClientDto>($"api/clients/{id}");
            if (dto == null) return NotFound();
            return View(ClientMapper.ToModel(dto));
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ok = await _api.DeleteAsync($"api/clients/{id}");
            TempData[ok ? "Success" : "Error"] =
                ok ? "Client deleted." : "Cannot delete — client has contracts.";

            return RedirectToAction(nameof(Index));
        }
    }
}
