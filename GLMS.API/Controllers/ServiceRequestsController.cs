using GLMS.API.Data;
using GLMS.Shared.Models;
using GLMS.Shared.Models.DTOs;
using GLMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API.Controllers
{
 


        /// <summary>Service requests linked to active contracts.</summary>
        [ApiController, Route("api/[controller]")]
        public class ServiceRequestsController : ControllerBase
        {
            private readonly ApplicationDbAPIContext _db;
            private readonly ICurrencyService _fx;
            public ServiceRequestsController(ApplicationDbAPIContext db, ICurrencyService fx) { _db = db; _fx = fx; }

            /// <summary>Get all service requests.</summary>
            [HttpGet, AllowAnonymous]
            public async Task<IActionResult> GetAll() =>
                Ok(await _db.ServiceRequests.Include(sr => sr.Contract).ThenInclude(c => c!.Client).OrderByDescending(sr => sr.CreatedOn).Select(sr => ToDto(sr)).ToListAsync());

            /// <summary>Get service request by ID.</summary>
            [HttpGet("{id:int}"), AllowAnonymous]
            public async Task<IActionResult> GetById(int id)
            {
                var sr = await _db.ServiceRequests.Include(x => x.Contract).ThenInclude(c => c!.Client).FirstOrDefaultAsync(x => x.Id == id);
                if (sr == null) return NotFound();
                return Ok(ToDto(sr));
            }

            /// <summary>Create a service request. Validates contract status and converts USD to ZAR.</summary>
            [HttpPost]
            public async Task<IActionResult> Create([FromBody] CreateServiceRequestRequest req)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var contract = await _db.Contracts.FindAsync(req.ContractId);
                if (contract == null) return BadRequest(new { message = "Contract not found." });
                if (!contract.CanCreateServiceRequest)
                    return BadRequest(new { message = $"Cannot create SR for contract with status '{contract.Status}'. Must be Active or Draft." });
                var rate = await _fx.GetUsdToZarRateAsync();
                var costZar = _fx.ConvertUsdToZar(req.CostUsd, rate);
                var sr = new ServiceRequest { ContractId = req.ContractId, Description = req.Description, CostUsd = req.CostUsd, CostZar = costZar, ExchangeRateUsed = rate, Status = ServiceRequestStatus.Pending, CreatedOn = DateTime.UtcNow };
                _db.ServiceRequests.Add(sr);
                await _db.SaveChangesAsync();
                var created = await _db.ServiceRequests.Include(x => x.Contract).ThenInclude(c => c!.Client).FirstAsync(x => x.Id == sr.Id);
                return CreatedAtAction(nameof(GetById), new { id = sr.Id }, ToDto(created));
            }

            /// <summary>Update status of a service request.</summary>
            [HttpPatch("{id:int}/status")]
            public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateContractStatusRequest req)
            {
                if (!Enum.TryParse<ServiceRequestStatus>(req.Status, true, out var s)) return BadRequest(new { message = $"Invalid status." });
                var sr = await _db.ServiceRequests.Include(x => x.Contract).ThenInclude(c => c!.Client).FirstOrDefaultAsync(x => x.Id == id);
                if (sr == null) return NotFound();
                sr.Status = s;
                await _db.SaveChangesAsync();
                return Ok(ToDto(sr));
            }

            /// <summary>Delete a service request.</summary>
            [HttpDelete("{id:int}")]
            public async Task<IActionResult> Delete(int id)
            {
                var sr = await _db.ServiceRequests.FindAsync(id);
                if (sr == null) return NotFound();
                _db.ServiceRequests.Remove(sr);
                await _db.SaveChangesAsync();
                return NoContent();
            }
        /// <summary>Update a service request.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceRequestRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sr = await _db.ServiceRequests
                .Include(x => x.Contract)
                .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sr == null)
                return NotFound();

            // Validate contract
            var contract = await _db.Contracts.FindAsync(req.ContractId);
            if (contract == null)
                return BadRequest(new { message = "Contract not found." });

            // Validate status
            if (!Enum.TryParse<ServiceRequestStatus>(req.Status, true, out var newStatus))
                return BadRequest(new { message = "Invalid status." });

            // Determine exchange rate
            var rate = req.ExchangeRate > 0
                ? req.ExchangeRate
                : await _fx.GetUsdToZarRateAsync();

            // Recalculate ZAR
            var costZar = _fx.ConvertUsdToZar(req.CostUsd, rate);

            // Apply updates
            sr.ContractId = req.ContractId;
            sr.Description = req.Description;
            sr.CostUsd = req.CostUsd;
            sr.CostZar = costZar;
            sr.ExchangeRateUsed = rate;
            sr.Status = newStatus;

            await _db.SaveChangesAsync();

            return Ok(ToDto(sr));
        }


        private static ServiceRequestDto ToDto(ServiceRequest sr) => new() { Id = sr.Id, ContractId = sr.ContractId, ClientName = sr.Contract?.Client?.Name ?? string.Empty, Description = sr.Description, CostUsd = sr.CostUsd, CostZar = sr.CostZar, ExchangeRateUsed = sr.ExchangeRateUsed, Status = sr.Status.ToString(), CreatedOn = sr.CreatedOn };
        }

        /// <summary>Live currency exchange rates.</summary>
        [ApiController, Route("api/[controller]")]
        public class CurrencyController : ControllerBase
        {
            private readonly ICurrencyService _fx;
            public CurrencyController(ICurrencyService fx) => _fx = fx;

            /// <summary>Get current USD to ZAR rate.</summary>
            [HttpGet("rate")]
            public async Task<IActionResult> GetRate() =>
                Ok(new CurrencyRateResponse { Rate = await _fx.GetUsdToZarRateAsync(), Base = "USD", Target = "ZAR", FetchedAt = DateTime.UtcNow });
        }

        /// <summary>Dashboard summary statistics.</summary>
        [ApiController, Route("api/[controller]")]
        public class DashboardController : ControllerBase
        {
            private readonly ApplicationDbAPIContext _db;
            public DashboardController(ApplicationDbAPIContext db) => _db = db;

            /// <summary>Aggregate counts for the GLMS dashboard.</summary>
            [HttpGet, AllowAnonymous]
            public async Task<IActionResult> Get() =>
                Ok(new DashboardDto
                {
                    TotalClients = await _db.Clients.CountAsync(),
                    TotalContracts = await _db.Contracts.CountAsync(),
                    ActiveContracts = await _db.Contracts.CountAsync(c => c.Status == ContractStatus.Active),
                    ExpiredContracts = await _db.Contracts.CountAsync(c => c.Status == ContractStatus.Expired),
                    TotalServiceRequests = await _db.ServiceRequests.CountAsync(),
                    PendingRequests = await _db.ServiceRequests.CountAsync(sr => sr.Status == ServiceRequestStatus.Pending)
                });
        }
    
}
