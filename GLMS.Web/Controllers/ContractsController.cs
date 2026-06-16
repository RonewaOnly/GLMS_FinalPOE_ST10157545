using GLMS.Shared.Models;
using GLMS.Shared.Models.DTOs;
using GLMS.Web.Mapping;
using GLMS.Web.Models.ViewModels;

using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web.Controllers
{
    public class ContractsController : Controller
    {
        private readonly ApiClient _api;
        public ContractsController(ApiClient api) => _api = api;
        public async Task<IActionResult> Index(ContractStatus? status, DateTime? startDateFrom, DateTime? startDateTo)
        {
            var qs = $"?status={status}&from={startDateFrom:yyyy-MM-dd}&to={startDateTo:yyyy-MM-dd}";
            var dtoList = await _api.GetAsync<List<ContractDto>>($"api/contracts{qs}") ?? new();

            var vm = new ContractFilterViewModel
            {
                Status = status,
                StartDateFrom = startDateFrom,
                StartDateTo = startDateTo,
                Results = dtoList.Select(ContractMapper.ToModel).ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var dto = await _api.GetAsync<ContractDto>($"api/contracts/{id}");
            if (dto == null) return NotFound();

            var model = ContractMapper.ToModel(dto);
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var clients = await _api.GetAsync<List<ClientDto>>("api/clients") ?? new();

            var vm = new ContractFormViewModel
            {
                ClientList = clients.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
            };

            return View(vm);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.ClientList = (await _api.GetAsync<List<ClientDto>>("api/clients") ?? new())
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
                return View(vm);
            }

            var (ok, data, err) = await _api.PostAsync<ContractDto>("api/contracts", new
            {
                vm.ClientId,
                vm.StartDate,
                vm.EndDate,
                Status = vm.Status,
                vm.ServiceLevel
            });

            if (!ok || data == null)
            {
                ModelState.AddModelError("", err ?? "Error.");
                vm.ClientList = (await _api.GetAsync<List<ClientDto>>("api/clients") ?? new())
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
                return View(vm);
            }

            if (vm.SignedAgreement?.Length > 0)
                await _api.UploadFileAsync($"api/contracts/{data.Id}/agreement", vm.SignedAgreement);

            TempData["Success"] = "Contract created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _api.GetAsync<ContractDto>($"api/contracts/{id}");
            if (dto == null) return NotFound();

            var clients = await _api.GetAsync<List<ClientDto>>("api/clients") ?? new();

            var vm = ContractMapper.ToFormModel(
                dto,
                clients.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            );

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContractFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.ClientList = (await _api.GetAsync<List<ClientDto>>("api/clients") ?? new())
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
                return View(vm);
            }

            var (ok, err) = await _api.PutAsync($"api/contracts/{id}", new
            {
                vm.ClientId,
                vm.StartDate,
                vm.EndDate,
                Status = vm.Status,
                vm.ServiceLevel
            });

            if (!ok)
            {
                ModelState.AddModelError("", err ?? "Update failed.");
                vm.ClientList = (await _api.GetAsync<List<ClientDto>>("api/clients") ?? new())
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
                return View(vm);
            }

            if (vm.SignedAgreement?.Length > 0)
                await _api.UploadFileAsync($"api/contracts/{id}/agreement", vm.SignedAgreement);

            TempData["Success"] = "Contract updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var (ok, err) = await _api.PatchAsync($"api/contracts/{id}/status", new { Status = status });
            TempData[ok ? "Success" : "Error"] = ok ? $"Status updated to {status}." : err;
            return RedirectToAction(nameof(Details), new { id });
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var dto = await _api.GetAsync<ContractDto>($"api/contracts/{id}");
            if (dto == null) return NotFound();

            var vm = ContractMapper.ToModel(dto);
            return View(vm);
        }
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _api.DeleteAsync($"api/contracts/{id}");
            TempData["Success"] = "Contract deleted.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Download(int id)
        {
            var (stream, ct, fn) = await _api.DownloadFileAsync($"api/contracts/{id}/agreement"); if (stream == null) return NotFound(); return File(stream, ct!, fn!);
        }
    }
    }
