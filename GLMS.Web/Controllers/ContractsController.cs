using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Models.ViewModels;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers
{
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        public ContractsController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }
        // GET: /Contracts  — with optional search/filter
        public async Task<IActionResult> Index(ContractFilterViewModel filter)
        {
            // Base query with eager-loaded client
            IQueryable<Contract> query = _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests);
            //  LINQ filter by date range
            if (filter.StartDateFrom.HasValue)
                query = query.Where(c => c.StartDate >= filter.StartDateFrom.Value);
            if (filter.StartDateTo.HasValue)
                query = query.Where(c => c.StartDate <= filter.StartDateTo.Value);
            // LINQ filter by status
            if (filter.Status.HasValue)
                query = query.Where(c => c.Status == filter.Status.Value);
            filter.Results = await query.OrderByDescending(c => c.StartDate).ToListAsync();
            return View(filter);
        }
        // GET: /Contracts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (contract == null) return NotFound();
            return View(contract);
        }
        // GET: /Contracts/Create
        public async Task<IActionResult> Create()
        {
            var vm = new ContractFormViewModel
            {
                ClientList = await GetClientSelectListAsync()
            };
            return View(vm);
        }
        // POST: /Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractFormViewModel vm)
        {
            // File is mandatory on create
            if (vm.SignedAgreement == null || vm.SignedAgreement.Length == 0)
                ModelState.AddModelError(nameof(vm.SignedAgreement),
                    "A signed agreement PDF is required.");
            if (!ModelState.IsValid)
            {
                vm.ClientList = await GetClientSelectListAsync();
                return View(vm);
            }
            // Save PDF file
            string savedPath = string.Empty;
            string savedFileName = string.Empty;
            try
            {
                (savedPath, savedFileName) = await _fileService.SaveAgreementAsync(vm.SignedAgreement!);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(vm.SignedAgreement), ex.Message);
                vm.ClientList = await GetClientSelectListAsync();
                return View(vm);
            }
            var contract = new Contract
            {
                ClientId = vm.ClientId,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                Status = vm.Status,
                ServiceLevel = vm.ServiceLevel,
                SignedAgreementPath = savedPath,
                SignedAgreementFileName = savedFileName,
                CreatedOn = DateTime.UtcNow
            };
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Contract created successfully.";
            return RedirectToAction(nameof(Index));
        }
        // GET: /Contracts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            var vm = new ContractFormViewModel
            {
                Id = contract.Id,
                ClientId = contract.ClientId,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status,
                ServiceLevel = contract.ServiceLevel,
                ExistingFileName = contract.SignedAgreementFileName,
                ClientList = await GetClientSelectListAsync()
            };
            return View(vm);
        }
        // POST: /Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContractFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            // On edit, file upload is optional — only validate if a new file was provided
            if (!ModelState.IsValid)
            {
                vm.ClientList = await GetClientSelectListAsync();
                return View(vm);
            }
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            // Replace PDF only if a new one was uploaded
            if (vm.SignedAgreement != null && vm.SignedAgreement.Length > 0)
            {
                try
                {
                    // Delete old file
                    if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
                        _fileService.DeleteAgreement(contract.SignedAgreementPath);
                    (contract.SignedAgreementPath, contract.SignedAgreementFileName) =
                        await _fileService.SaveAgreementAsync(vm.SignedAgreement);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(nameof(vm.SignedAgreement), ex.Message);
                    vm.ClientList = await GetClientSelectListAsync();
                    return View(vm);
                }
            }
            contract.ClientId = vm.ClientId;
            contract.StartDate = vm.StartDate;
            contract.EndDate = vm.EndDate;
            contract.Status = vm.Status;
            contract.ServiceLevel = vm.ServiceLevel;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Contract updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        // GET: /Contracts/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (contract == null) return NotFound();
            return View(contract);
        }
        // POST: /Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            // Remove associated PDF
            if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
                _fileService.DeleteAgreement(contract.SignedAgreementPath);
            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Contract deleted.";
            return RedirectToAction(nameof(Index));
        }
        // GET: /Contracts/Download/5  — serves the PDF file
        public async Task<IActionResult> Download(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
                return NotFound();
            var fullPath = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot",
                contract.SignedAgreementPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(fullPath))
                return NotFound("The file no longer exists on the server.");
            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(bytes, "application/pdf",
                contract.SignedAgreementFileName ?? "agreement.pdf");
        }
        //  Helper method to get clients for dropdown
        private async Task<IEnumerable<SelectListItem>> GetClientSelectListAsync() =>
            await _context.Clients
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
    }
}
