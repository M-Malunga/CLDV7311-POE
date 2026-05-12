using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ST10296771_CLDV7311_POE.Data;
using ST10296771_CLDV7311_POE.Models;
using ST10296771_CLDV7311_POE.Services;

namespace ST10296771_CLDV7311_POE.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EventsController(
            ApplicationDbContext context,
            IBlobStorageService blobStorageService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _blobStorageService = blobStorageService;
            _httpContextAccessor = httpContextAccessor;
        }

        private bool IsEmployeeOrAdmin()
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("UserRole");
            return role == "Employee" || role == "Admin";
        }

        public async Task<IActionResult> Index()
        {
            var events = _context.Events.Include(e => e.Venue);
            return View(await events.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound();

            return View(ev);
        }

        public IActionResult Create()
        {
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to create events.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventItem, IFormFile imageFile)
        {
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to create events.";
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && _blobStorageService.IsValidImage(imageFile))
                {
                    var fileName = await _blobStorageService.UploadImageAsync(imageFile, imageFile.FileName);
                    eventItem.ImageFileName = fileName;
                    eventItem.ImageContentType = imageFile.ContentType;
                }

                _context.Add(eventItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", eventItem.VenueId);
            return View(eventItem);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to edit events.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return NotFound();

            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return NotFound();

            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", eventItem.VenueId);
            return View(eventItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event eventItem, IFormFile imageFile)
        {
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to edit events.";
                return RedirectToAction("Index", "Home");
            }

            if (id != eventItem.EventId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEvent = await _context.Events.FindAsync(id);
                    if (existingEvent == null) return NotFound();

                    // Handle image update
                    if (imageFile != null && _blobStorageService.IsValidImage(imageFile))
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingEvent.ImageFileName))
                        {
                            await _blobStorageService.DeleteImageAsync(existingEvent.ImageFileName);
                        }

                        // Upload new image
                        var fileName = await _blobStorageService.UploadImageAsync(imageFile, imageFile.FileName);
                        eventItem.ImageFileName = fileName;
                        eventItem.ImageContentType = imageFile.ContentType;
                    }
                    else
                    {
                        // Keep existing image
                        eventItem.ImageFileName = existingEvent.ImageFileName;
                        eventItem.ImageContentType = existingEvent.ImageContentType;
                    }

                    _context.Entry(existingEvent).CurrentValues.SetValues(eventItem);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventItem.EventId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", eventItem.VenueId);
            return View(eventItem);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to delete events.";
                return RedirectToAction("Index", "Home");
            }

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
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to delete events.";
                return RedirectToAction("Index", "Home");
            }

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
                // Delete image from Azure Blob Storage
                if (!string.IsNullOrEmpty(eventItem.ImageFileName))
                {
                    await _blobStorageService.DeleteImageAsync(eventItem.ImageFileName);
                }

                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}