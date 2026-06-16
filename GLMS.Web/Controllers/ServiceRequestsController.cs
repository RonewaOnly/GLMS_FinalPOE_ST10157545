using GLMS.Shared.Models;
using GLMS.Shared.Models.DTOs;
using GLMS.Web.Mapping;
using GLMS.Web.Models;
using GLMS.Web.Models.ViewModels;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CurrencyRateResponse = GLMS.Shared.Models.DTOs.CurrencyRateResponse;

namespace GLMS.Web.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApiClient _api;
        public ServiceRequestsController(ApiClient api) => _api = api;
        public async Task<IActionResult> Index()
        {
            var dtoList = await _api.GetAsync<List<ServiceRequestDto>>("api/servicerequests") ?? new();

            var model = dtoList.Select(ServiceRequestMapper.ToModel).ToList();

            return View(model);
        }
        public async Task<IActionResult> Details(int id) { var sr = await _api.GetAsync<ServiceRequestDto>($"api/servicerequests/{id}"); if (sr == null) return NotFound(); var model = ServiceRequestMapper.ToModel(sr);
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Create(int? contractId)
        {
            var rate = await _api.GetAsync<CurrencyRateResponse>("api/currency/rate");
            var contracts = await _api.GetAsync<List<ContractDto>>("api/contracts?status=Active") ?? new();

            var vm = new ServiceRequestFormViewModel
            {
                ContractId = contractId ?? 0,
                ExchangeRate = rate?.Rate ?? 18.50m,
                ContractList = contracts.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.ClientName} (#{c.Id})"
                })
            };

            return View(vm);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewBag(vm);
                return View(vm);
            }

            var (ok, _, err) = await _api.PostAsync<ServiceRequestDto>("api/servicerequests", new
            {
                vm.ContractId,
                vm.Description,
                vm.CostUsd,
                vm.ExchangeRate
            });

            if (!ok)
            {
                ModelState.AddModelError("", err ?? "Failed.");
                await PopulateViewBag(vm);
                return View(vm);
            }

            TempData["Success"] = "Service request created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet] public async Task<IActionResult> Delete(int id) { var sr = await _api.GetAsync<ServiceRequestDto>($"api/servicerequests/{id}"); if (sr == null) return NotFound(); var model = ServiceRequestMapper.ToModel(sr);
            return View(model);
        }
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) { await _api.DeleteAsync($"api/servicerequests/{id}"); TempData["Success"] = "Deleted."; return RedirectToAction(nameof(Index)); }
        [HttpGet] public async Task<IActionResult> GetRate() { var r = await _api.GetAsync<CurrencyRateResponse>("api/currency/rate"); return Json(new { rate = r?.Rate ?? 18.50m }); }
        private async Task PopulateViewBag(ServiceRequestFormViewModel vm) { var r = await _api.GetAsync<CurrencyRateResponse>("api/currency/rate"); ViewBag.Rate = r?.Rate ?? 18.50m; ViewBag.Contracts = await _api.GetAsync<List<ContractDto>>("api/contracts?status=Active") ?? new(); }

    }
}
