using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Models.ViewModels;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;
        public ServiceRequestsController(
            ApplicationDbContext context,
            ICurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }
        // GET: /ServiceRequests
        public async Task<IActionResult> Index()
        {
            var requests = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                    .ThenInclude(c => c!.Client)
                .OrderByDescending(sr => sr.CreatedOn)
                .ToListAsync();
            return View(requests);
        }
        // GET: /ServiceRequests/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var sr = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                    .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(sr => sr.Id == id);
            if (sr == null) return NotFound();
            return View(sr);
        }
        // GET: /ServiceRequests/Create?contractId=1
        public async Task<IActionResult> Create(int? contractId)
        {
            var rate = await _currencyService.GetUsdToZarRateAsync();
            var vm = new ServiceRequestFormViewModel
            {
                ExchangeRate = rate,
                ContractList = await GetActiveContractSelectListAsync()
            };
            // Pre-select contract if passed via query string
            if (contractId.HasValue)
            {
                var contract = await _context.Contracts
                    .Include(c => c.Client)
                    .FirstOrDefaultAsync(c => c.Id == contractId.Value);
                if (contract != null)
                {
                    //  WORKFLOW CHECK 
                    if (!contract.CanCreateServiceRequest)
                    {
                        TempData["Error"] =
                            $"Cannot raise a service request on a contract with status '{contract.Status}'. " +
                            "Only Active or Draft contracts are permitted.";
                        return RedirectToAction("Details", "Contracts", new { id = contractId });
                    }
                    vm.ContractId = contract.Id;
                    vm.ContractInfo = $"{contract.Client?.Name} — {contract.ServiceLevel} ({contract.Status})";
                }
            }
            return View(vm);
        }
        // POST: /ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.ContractList = await GetActiveContractSelectListAsync();
                vm.ExchangeRate = await _currencyService.GetUsdToZarRateAsync();
                return View(vm);
            }
            // Re-fetch and validate the parent contract server-side
            var contract = await _context.Contracts.FindAsync(vm.ContractId);
            if (contract == null)
            {
                ModelState.AddModelError("", "Selected contract does not exist.");
                vm.ContractList = await GetActiveContractSelectListAsync();
                return View(vm);
            }
            //  WORKFLOW ENFORCEMENT 
            if (!contract.CanCreateServiceRequest)
            {
                ModelState.AddModelError("",
                    $"Service requests cannot be created for contracts with status '{contract.Status}'. " +
                    "The contract must be Active or Draft.");
                vm.ContractList = await GetActiveContractSelectListAsync();
                vm.ExchangeRate = await _currencyService.GetUsdToZarRateAsync();
                return View(vm);
            }
            //  CURRENCY CONVERSION 
            var rate = await _currencyService.GetUsdToZarRateAsync();
            var zarCost = _currencyService.ConvertUsdToZar(vm.CostUsd, rate);
            var sr = new ServiceRequest
            {
                ContractId = vm.ContractId,
                Description = vm.Description,
                CostUsd = vm.CostUsd,
                CostZar = zarCost,
                ExchangeRateUsed = rate,
                Status = ServiceRequestStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };
            _context.ServiceRequests.Add(sr);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Service Request #{sr.Id} created. Cost: ${sr.CostUsd:N2} USD = R{sr.CostZar:N2} ZAR.";
            return RedirectToAction(nameof(Index));
        }
        // GET: /ServiceRequests/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var sr = await _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (sr == null) return NotFound();
            var vm = new ServiceRequestFormViewModel
            {
                Id = sr.Id,
                ContractId = sr.ContractId,
                Description = sr.Description,
                CostUsd = sr.CostUsd,
                CostZar = sr.CostZar,
                ExchangeRate = sr.ExchangeRateUsed,
                Status = sr.Status,
                ContractInfo = $"{sr.Contract?.Client?.Name} — {sr.Contract?.ServiceLevel}",
                ContractList = await GetActiveContractSelectListAsync()
            };
            return View(vm);
        }
        // POST: /ServiceRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceRequestFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                vm.ContractList = await GetActiveContractSelectListAsync();
                return View(vm);
            }
            var sr = await _context.ServiceRequests.FindAsync(id);
            if (sr == null) return NotFound();
            // Recalculate ZAR if USD amount changed
            var rate = await _currencyService.GetUsdToZarRateAsync();
            sr.ContractId = vm.ContractId;
            sr.Description = vm.Description;
            sr.CostUsd = vm.CostUsd;
            sr.CostZar = _currencyService.ConvertUsdToZar(vm.CostUsd, rate);
            sr.ExchangeRateUsed = rate;
            sr.Status = vm.Status;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Service request updated.";
            return RedirectToAction(nameof(Index));
        }
        // GET: /ServiceRequests/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var sr = await _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (sr == null) return NotFound();
            return View(sr);
        }
        // POST: /ServiceRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sr = await _context.ServiceRequests.FindAsync(id);
            if (sr == null) return NotFound();
            _context.ServiceRequests.Remove(sr);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Service request deleted.";
            return RedirectToAction(nameof(Index));
        }
        // GET: /ServiceRequests/GetRate  — AJAX endpoint for live conversion
        [HttpGet]
        public async Task<IActionResult> GetRate()
        {
            var rate = await _currencyService.GetUsdToZarRateAsync();
            return Json(new { rate });
        }
        //  Helper method to get active contracts for dropdown 
        private async Task<IEnumerable<SelectListItem>> GetActiveContractSelectListAsync() =>
            await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active || c.Status == ContractStatus.Draft)
                .OrderBy(c => c.Client!.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Client!.Name} — {c.ServiceLevel} [{c.Status}]"
                })
                .ToListAsync();
    }
}
