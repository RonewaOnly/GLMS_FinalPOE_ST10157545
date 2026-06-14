using GLMS.API.Models.DTOs;

using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web.Controllers
{
    public class ContractsController : Controller
    {
        private readonly ApiClient _api;
        public ContractsController(ApiClient api) => _api = api;
        public async Task<IActionResult> Index(string? status, DateTime? from, DateTime? to)
        {
            var qs = $"?status={status}&from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
            var list = await _api.GetAsync<List<ContractDto>>($"api/contracts{qs}");
            ViewBag.FilterStatus = status; ViewBag.FilterFrom = from; ViewBag.FilterTo = to;
            return View(list ?? new());
        }
        public async Task<IActionResult> Details(int id) { var c = await _api.GetAsync<ContractDto>($"api/contracts/{id}"); if (c == null) return NotFound(); return View(c); }
        [HttpGet] public async Task<IActionResult> Create() { ViewBag.Clients = await _api.GetAsync<List<ClientDto>>("api/clients") ?? new(); return View(new ContractDto()); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractDto vm, IFormFile? SignedAgreement)
        {
            if (!ModelState.IsValid) { ViewBag.Clients = await _api.GetAsync<List<ClientDto>>("api/clients") ?? new(); return View(vm); }
            var (ok, data, err) = await _api.PostAsync<ContractDto>("api/contracts", new { vm.ClientId, vm.StartDate, vm.EndDate, Status = vm.Status, vm.ServiceLevel });
            if (!ok || data == null) { ModelState.AddModelError("", err ?? "Error."); ViewBag.Clients = await _api.GetAsync<List<ClientDto>>("api/clients") ?? new(); return View(vm); }
            if (SignedAgreement?.Length > 0) await _api.UploadFileAsync($"api/contracts/{data.Id}/agreement", SignedAgreement);
            TempData["Success"] = "Contract created."; return RedirectToAction(nameof(Index));
        }
        [HttpGet] public async Task<IActionResult> Edit(int id) { var c = await _api.GetAsync<ContractDto>($"api/contracts/{id}"); if (c == null) return NotFound(); ViewBag.Clients = await _api.GetAsync<List<ClientDto>>("api/clients") ?? new(); return View(c); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContractDto vm, IFormFile? SignedAgreement)
        {
            if (!ModelState.IsValid) { ViewBag.Clients = await _api.GetAsync<List<ClientDto>>("api/clients") ?? new(); return View(vm); }
            var (ok, err) = await _api.PutAsync($"api/contracts/{id}", new { vm.ClientId, vm.StartDate, vm.EndDate, Status = vm.Status, vm.ServiceLevel });
            if (!ok) { ModelState.AddModelError("", err ?? "Update failed."); ViewBag.Clients = await _api.GetAsync<List<ClientDto>>("api/clients") ?? new(); return View(vm); }
            if (SignedAgreement?.Length > 0) await _api.UploadFileAsync($"api/contracts/{id}/agreement", SignedAgreement);
            TempData["Success"] = "Contract updated."; return RedirectToAction(nameof(Index));
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var (ok, err) = await _api.PatchAsync($"api/contracts/{id}/status", new { Status = status });
            TempData[ok ? "Success" : "Error"] = ok ? $"Status updated to {status}." : err;
            return RedirectToAction(nameof(Details), new { id });
        }
        [HttpGet] public async Task<IActionResult> Delete(int id) { var c = await _api.GetAsync<ContractDto>($"api/contracts/{id}"); if (c == null) return NotFound(); return View(c); }
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) { await _api.DeleteAsync($"api/contracts/{id}"); TempData["Success"] = "Contract deleted."; return RedirectToAction(nameof(Index)); }
        public async Task<IActionResult> Download(int id)
        {
            var (stream, ct, fn) = await _api.DownloadFileAsync($"api/contracts/{id}/agreement"); if (stream == null) return NotFound(); return File(stream, ct!, fn!);
        }
    }
    }
