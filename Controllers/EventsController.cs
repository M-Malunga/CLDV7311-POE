using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10296771_CLDV7311_POE.Data;

namespace ST10296771_CLDV7311_POE.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsEmployeeOrAdmin()) return Forbid();
            if (id == null) return NotFound();

            var eventItem = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null) return NotFound();

            return View(eventItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsEmployeeOrAdmin()) return Forbid();

            // CHECK FOR ACTIVE BOOKINGS
            var hasActiveBookings = await _context.Bookings
                .AnyAsync(b => b.EventId == id && b.BookingDate >= DateTime.Today);

            if (hasActiveBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete this event because it has active bookings (today or future).";
                return RedirectToAction(nameof(Index));
            }

            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem != null)
            {
                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool IsEmployeeOrAdmin()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "Employee" || role == "Admin";
        }
        public async Task<IActionResult> Index()
        {
            var events = _context.Events.Include(e => e.Venue);
            return View(await events.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            var ev = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventId == id);

            return View(ev);
        }
    }
}
