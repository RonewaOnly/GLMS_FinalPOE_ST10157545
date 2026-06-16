using GLMS.API.Data;
using GLMS.Shared.Models;
using GLMS.Shared.Models.DTOs;
using GLMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API.Controllers
{
 

        /// <summary>Manage freight contracts, PDF agreements and status transitions.</summary>
        [ApiController]
        [Route("api/[controller]")]
        [Authorize]
        public class ContractsController : ControllerBase
        {
            private readonly ApplicationDbAPIContext _db;
            private readonly IFileService _files;
            public ContractsController(ApplicationDbAPIContext db, IFileService files) { _db = db; _files = files; }

            /// <summary>Get contracts with optional ?status=, ?from=, ?to= filters.</summary>
            [HttpGet, AllowAnonymous]
            public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
            {
                IQueryable<Contract> q = _db.Contracts.Include(c => c.Client).Include(c => c.ServiceRequests);
                if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ContractStatus>(status, true, out var s)) q = q.Where(c => c.Status == s);
                if (from.HasValue) q = q.Where(c => c.StartDate >= from.Value);
                if (to.HasValue) q = q.Where(c => c.StartDate <= to.Value);
                return Ok(await q.OrderByDescending(c => c.StartDate).Select(c => ToDto(c)).ToListAsync());
            }

            /// <summary>Get single contract by ID.</summary>
            [HttpGet("{id:int}"), AllowAnonymous]
            public async Task<IActionResult> GetById(int id)
            {
                var c = await _db.Contracts.Include(x => x.Client).Include(x => x.ServiceRequests).FirstOrDefaultAsync(x => x.Id == id);
                if (c == null) return NotFound();
                return Ok(ToDto(c));
            }

            /// <summary>Create a contract. Requires JWT.</summary>
            [HttpPost]
            public async Task<IActionResult> Create([FromBody] CreateContractRequest req)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (!await _db.Clients.AnyAsync(c => c.Id == req.ClientId)) return BadRequest(new { message = "Client not found." });
                var contract = new Contract { ClientId = req.ClientId, StartDate = req.StartDate, EndDate = req.EndDate, Status = req.Status, ServiceLevel = req.ServiceLevel, SignedAgreementPath = req.AgreementPath, SignedAgreementFileName = req.AgreementFileName, CreatedOn = DateTime.UtcNow };
                _db.Contracts.Add(contract);
                await _db.SaveChangesAsync();
                var created = await _db.Contracts.Include(c => c.Client).Include(c => c.ServiceRequests).FirstAsync(c => c.Id == contract.Id);
                return CreatedAtAction(nameof(GetById), new { id = contract.Id }, ToDto(created));
            }

            /// <summary>Upload PDF agreement for a contract.</summary>
            [HttpPost("{id:int}/agreement")]
            public async Task<IActionResult> UploadAgreement(int id, IFormFile file)
            {
                var contract = await _db.Contracts.FindAsync(id);
                if (contract == null) return NotFound();
                try
                {
                    if (!string.IsNullOrEmpty(contract.SignedAgreementPath)) _files.DeleteAgreement(contract.SignedAgreementPath);
                    (contract.SignedAgreementPath, contract.SignedAgreementFileName) = await _files.SaveAgreementAsync(file);
                    await _db.SaveChangesAsync();
                    return Ok(new { path = contract.SignedAgreementPath, fileName = contract.SignedAgreementFileName });
                }
                catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            }

            /// <summary>Download signed agreement PDF.</summary>
            [HttpGet("{id:int}/agreement"), AllowAnonymous]
            public async Task<IActionResult> DownloadAgreement(int id)
            {
                var contract = await _db.Contracts.FindAsync(id);
                if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath)) return NotFound();
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", contract.SignedAgreementPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (!System.IO.File.Exists(fullPath)) return NotFound("File not found on server.");
                return File(await System.IO.File.ReadAllBytesAsync(fullPath), "application/pdf", contract.SignedAgreementFileName ?? "agreement.pdf");
            }

            /// <summary>Update contract status (PATCH). Valid values: Draft, Active, Expired, OnHold.</summary>
            [HttpPatch("{id:int}/status")]
            public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateContractStatusRequest req)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (!Enum.TryParse<ContractStatus>(req.Status, true, out var newStatus)) return BadRequest(new { message = $"Invalid status '{req.Status}'." });
                var contract = await _db.Contracts.Include(c => c.Client).Include(c => c.ServiceRequests).FirstOrDefaultAsync(c => c.Id == id);
                if (contract == null) return NotFound();
                contract.Status = newStatus;
                await _db.SaveChangesAsync();
                return Ok(ToDto(contract));
            }

            /// <summary>Update all fields of a contract.</summary>
            [HttpPut("{id:int}")]
            public async Task<IActionResult> Update(int id, [FromBody] CreateContractRequest req)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var contract = await _db.Contracts.FindAsync(id);
                if (contract == null) return NotFound();
                contract.ClientId = req.ClientId; contract.StartDate = req.StartDate; contract.EndDate = req.EndDate; contract.Status = req.Status; contract.ServiceLevel = req.ServiceLevel;
                await _db.SaveChangesAsync();
                return NoContent();
            }

            /// <summary>Delete a contract and its agreement file.</summary>
            [HttpDelete("{id:int}")]
            public async Task<IActionResult> Delete(int id)
            {
                var contract = await _db.Contracts.FindAsync(id);
                if (contract == null) return NotFound();
                if (!string.IsNullOrEmpty(contract.SignedAgreementPath)) _files.DeleteAgreement(contract.SignedAgreementPath);
                _db.Contracts.Remove(contract);
                await _db.SaveChangesAsync();
                return NoContent();
            }

            private static ContractDto ToDto(Contract c) => new()
            {
                Id = c.Id,
                ClientId = c.ClientId,
                ClientName = c.Client?.Name ?? string.Empty,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status.ToString(),
                ServiceLevel = c.ServiceLevel,
                SignedAgreementFileName = c.SignedAgreementFileName,
                HasAgreement = !string.IsNullOrEmpty(c.SignedAgreementPath),
                CanCreateServiceRequest = c.CanCreateServiceRequest,
                CreatedOn = c.CreatedOn,
                ServiceRequests = c.ServiceRequests.Select(sr => new ServiceRequestDto { Id = sr.Id, ContractId = sr.ContractId, Description = sr.Description, CostUsd = sr.CostUsd, CostZar = sr.CostZar, ExchangeRateUsed = sr.ExchangeRateUsed, Status = sr.Status.ToString(), CreatedOn = sr.CreatedOn }).ToList()
            };
        }
    

}
