using GLMS.API.Models.DTOs;
using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Models.ViewModels;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CurrencyRateResponse = GLMS.Web.Models.CurrencyRateResponse;

namespace GLMS.Web.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApiClient _api;
        public ServiceRequestsController(ApiClient api) => _api = api;
        public async Task<IActionResult> Index() => View(await _api.GetAsync<List<ServiceRequestDto>>("api/servicerequests") ?? new());
        public async Task<IActionResult> Details(int id) { var sr = await _api.GetAsync<ServiceRequestDto>($"api/servicerequests/{id}"); if (sr == null) return NotFound(); return View(sr); }
        [HttpGet]
        public async Task<IActionResult> Create(int? contractId)
        {
            var rate = await _api.GetAsync<CurrencyRateResponse>("api/currency/rate");
            ViewBag.Rate = rate?.Rate ?? 18.50m;
            ViewBag.Contracts = await _api.GetAsync<List<ContractDto>>("api/contracts?status=Active") ?? new();
            ViewBag.ContractId = contractId;
            return View(new ServiceRequestDto());
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestDto vm)
        {
            if (!ModelState.IsValid) { await PopulateViewBag(); return View(vm); }
            var (ok, _, err) = await _api.PostAsync<ServiceRequestDto>("api/servicerequests", new { vm.ContractId, vm.Description, vm.CostUsd });
            if (!ok) { ModelState.AddModelError("", err ?? "Failed."); await PopulateViewBag(); return View(vm); }
            TempData["Success"] = "Service request created."; return RedirectToAction(nameof(Index));
        }
        [HttpGet] public async Task<IActionResult> Delete(int id) { var sr = await _api.GetAsync<ServiceRequestDto>($"api/servicerequests/{id}"); if (sr == null) return NotFound(); return View(sr); }
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) { await _api.DeleteAsync($"api/servicerequests/{id}"); TempData["Success"] = "Deleted."; return RedirectToAction(nameof(Index)); }
        [HttpGet] public async Task<IActionResult> GetRate() { var r = await _api.GetAsync<CurrencyRateResponse>("api/currency/rate"); return Json(new { rate = r?.Rate ?? 18.50m }); }
        private async Task PopulateViewBag() { var r = await _api.GetAsync<CurrencyRateResponse>("api/currency/rate"); ViewBag.Rate = r?.Rate ?? 18.50m; ViewBag.Contracts = await _api.GetAsync<List<ContractDto>>("api/contracts?status=Active") ?? new(); }

    }
}
