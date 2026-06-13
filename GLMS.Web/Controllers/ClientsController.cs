using GLMS.Web.Data;
using GLMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: /Clients
        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients
                .Include(c => c.Contracts)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(clients);
        }
        // GET: /Clients/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Contracts)
                    .ThenInclude(ct => ct.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();
            return View(client);
        }
        // GET: /Clients/Create
        public IActionResult Create() => View();
        // POST: /Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (!ModelState.IsValid) return View(client);
            client.CreatedOn = DateTime.UtcNow;
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Client '{client.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }
        // GET: /Clients/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }
        // POST: /Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Id) return BadRequest();
            if (!ModelState.IsValid) return View(client);
            try
            {
                _context.Update(client);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Client updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Clients.Any(c => c.Id == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: /Clients/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();
            return View(client);
        }
        // POST: /Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            if (_context.Contracts.Any(c => c.ClientId == id))
            {
                TempData["Error"] = "Cannot delete a client that has existing contracts.";
                return RedirectToAction(nameof(Index));
            }
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Client deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
