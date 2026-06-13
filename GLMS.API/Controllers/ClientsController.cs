
using GLMS.API.Data;
using GLMS.API.Models;
using GLMS.API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace GLMS.API.Controllers
{
        /// <summary>Manage logistics clients.</summary>
        [ApiController]
        [Route("api/[controller]")]
        [Authorize]
        public class ClientsController : ControllerBase
        {
            private readonly ApplicationDbAPIContext _db;
            public ClientsController(ApplicationDbAPIContext db) => _db = db;

            /// <summary>Get all clients.</summary>
            [HttpGet, AllowAnonymous]
            public async Task<IActionResult> GetAll() =>
                Ok(await _db.Clients.Include(c => c.Contracts).OrderBy(c => c.Name)
                    .Select(c => new ClientDto { Id = c.Id, Name = c.Name, ContractDetails = c.ContractDetails, Region = c.Region, CreatedOn = c.CreatedOn, ContractCount = c.Contracts.Count })
                    .ToListAsync());

            /// <summary>Get client by ID.</summary>
            [HttpGet("{id:int}"), AllowAnonymous]
            public async Task<IActionResult> GetById(int id)
            {
                var c = await _db.Clients.Include(x => x.Contracts).FirstOrDefaultAsync(x => x.Id == id);
                if (c == null) return NotFound();
                return Ok(new ClientDto { Id = c.Id, Name = c.Name, ContractDetails = c.ContractDetails, Region = c.Region, CreatedOn = c.CreatedOn, ContractCount = c.Contracts.Count });
            }

            /// <summary>Create a client. Requires JWT.</summary>
            [HttpPost]
            public async Task<IActionResult> Create([FromBody] CreateClientRequest req)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var client = new Client { Name = req.Name, ContractDetails = req.ContractDetails, Region = req.Region, CreatedOn = DateTime.UtcNow };
                _db.Clients.Add(client);
                await _db.SaveChangesAsync();
                var dto = new ClientDto { Id = client.Id, Name = client.Name, ContractDetails = client.ContractDetails, Region = client.Region, CreatedOn = client.CreatedOn };
                return CreatedAtAction(nameof(GetById), new { id = client.Id }, dto);
            }

            /// <summary>Update a client.</summary>
            [HttpPut("{id:int}")]
            public async Task<IActionResult> Update(int id, [FromBody] CreateClientRequest req)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var client = await _db.Clients.FindAsync(id);
                if (client == null) return NotFound();
                client.Name = req.Name; client.ContractDetails = req.ContractDetails; client.Region = req.Region;
                await _db.SaveChangesAsync();
                return NoContent();
            }

            /// <summary>Delete a client (no contracts allowed).</summary>
            [HttpDelete("{id:int}")]
            public async Task<IActionResult> Delete(int id)
            {
                var client = await _db.Clients.FindAsync(id);
                if (client == null) return NotFound();
                if (await _db.Contracts.AnyAsync(c => c.ClientId == id))
                    return BadRequest(new { message = "Cannot delete a client with existing contracts." });
                _db.Clients.Remove(client);
                await _db.SaveChangesAsync();
                return NoContent();
            }
        }
    
}
