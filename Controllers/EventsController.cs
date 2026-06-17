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
        private readonly ILogger<EventsController> _logger;

        public EventsController(
            ApplicationDbContext context,
            IBlobStorageService blobStorageService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<EventsController> logger)
        {
            _context = context;
            _blobStorageService = blobStorageService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private bool IsEmployeeOrAdmin()
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("UserRole") ?? string.Empty;
            return role == "Employee" || role == "Admin";
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var events = await _context.Events
                    .Include(e => e.Venue)
                    .Include(e => e.EventType)
                    .Select(e => new Event
                    {
                        EventId = e.EventId,
                        EventName = e.EventName ?? "Untitled Event",
                        EventDate = e.EventDate,
                        Description = e.Description ?? "",
                        VenueId = e.VenueId,
                        Venue = e.Venue != null ? new Venue { VenueName = e.Venue.VenueName ?? "TBA" } : null,
                        ExpectedAttendees = e.ExpectedAttendees,
                        OrganizerName = e.OrganizerName ?? "Unknown",
                        OrganizerContact = e.OrganizerContact ?? "Not Provided",
                        ImageFileName = e.ImageFileName ?? "",
                        ImageContentType = e.ImageContentType ?? "",
                        EventTypeId = e.EventTypeId,
                        EventType = e.EventType != null ? new EventType
                        {
                            CategoryName = e.EventType.CategoryName ?? "Uncategorized",
                            IconClass = e.EventType.IconClass ?? "bi-calendar"
                        } : null
                    })
                    .ToListAsync();

                return View(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading events");
                TempData["ErrorMessage"] = "Error loading events. Please try again.";
                return View(new List<Event>());
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Use a projection to handle NULL values safely
                var eventItem = await _context.Events
                    .Where(e => e.EventId == id)
                    .Select(e => new Event
                    {
                        EventId = e.EventId,
                        EventName = e.EventName ?? "Untitled Event",
                        EventDate = e.EventDate,
                        Description = e.Description ?? "",
                        VenueId = e.VenueId,
                        ExpectedAttendees = e.ExpectedAttendees,
                        OrganizerName = e.OrganizerName ?? "Unknown Organizer",
                        OrganizerContact = e.OrganizerContact ?? "Not Provided",
                        ImageFileName = e.ImageFileName ?? "",
                        ImageContentType = e.ImageContentType ?? "",
                        EventTypeId = e.EventTypeId,
                        IsPublic = e.IsPublic,
                        TicketPrice = e.TicketPrice,
                        MaxCapacity = e.MaxCapacity,
                        // Handle Venue with NULL checks
                        Venue = e.Venue == null ? null : new Venue
                        {
                            VenueId = e.Venue.VenueId,
                            VenueName = e.Venue.VenueName ?? "Unknown Venue",
                            Location = e.Venue.Location ?? "Unknown Location",
                            Capacity = e.Venue.Capacity,
                            ContactPhone = e.Venue.ContactPhone ?? "Not Provided",
                            ContactEmail = e.Venue.ContactEmail ?? "Not Provided",
                            IsAvailable = e.Venue.IsAvailable,
                            OperatingHours = e.Venue.OperatingHours ?? "Not specified",
                            HasParking = e.Venue.HasParking,
                            IsIndoor = e.Venue.IsIndoor,
                            IsWheelchairAccessible = e.Venue.IsWheelchairAccessible
                        },
                        // Handle EventType with NULL checks
                        EventType = e.EventType == null ? null : new EventType
                        {
                            EventTypeId = e.EventType.EventTypeId,
                            CategoryName = e.EventType.CategoryName ?? "Uncategorized",
                            Description = e.EventType.Description ?? "",
                            IconClass = e.EventType.IconClass ?? "bi-calendar-event"
                        }
                    })
                    .FirstOrDefaultAsync();

                if (eventItem == null)
                {
                    return NotFound();
                }

                return View(eventItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading event details for ID: {id}");
                TempData["ErrorMessage"] = "Unable to load event details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to create events.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName");
            ViewBag.EventTypeId = new SelectList(_context.EventTypes, "EventTypeId", "CategoryName");
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
                try
                {
                    // Handle image upload
                    if (imageFile != null && _blobStorageService.IsValidImage(imageFile))
                    {
                        var fileName = await _blobStorageService.UploadImageAsync(imageFile, imageFile.FileName);
                        eventItem.ImageFileName = fileName;
                        eventItem.ImageContentType = imageFile.ContentType;
                    }

                    // Ensure no NULL values
                    if (string.IsNullOrEmpty(eventItem.EventName)) eventItem.EventName = "Untitled Event";
                    if (string.IsNullOrEmpty(eventItem.OrganizerName)) eventItem.OrganizerName = "Unknown Organizer";
                    if (string.IsNullOrEmpty(eventItem.OrganizerContact)) eventItem.OrganizerContact = "Not Provided";
                    if (string.IsNullOrEmpty(eventItem.Description)) eventItem.Description = "";

                    _context.Add(eventItem);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating event");
                    ModelState.AddModelError("", "Error creating event. Please try again.");
                }
            }

            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", eventItem.VenueId);
            ViewBag.EventTypeId = new SelectList(_context.EventTypes, "EventTypeId", "CategoryName", eventItem.EventTypeId);
            return View(eventItem);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to edit events.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return NotFound();

            var eventItem = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null) return NotFound();

            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", eventItem.VenueId);
            ViewBag.EventTypeId = new SelectList(_context.EventTypes, "EventTypeId", "CategoryName", eventItem.EventTypeId);
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
                        if (!string.IsNullOrEmpty(existingEvent.ImageFileName))
                        {
                            await _blobStorageService.DeleteImageAsync(existingEvent.ImageFileName);
                        }
                        var fileName = await _blobStorageService.UploadImageAsync(imageFile, imageFile.FileName);
                        eventItem.ImageFileName = fileName;
                        eventItem.ImageContentType = imageFile.ContentType;
                    }
                    else
                    {
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
            ViewBag.EventTypeId = new SelectList(_context.EventTypes, "EventTypeId", "CategoryName", eventItem.EventTypeId);
            return View(eventItem);
        }

        // GET: Events/Delete/5
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
                .Include(e => e.EventType)
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

            try
            {
                var hasActiveBookings = await _context.Bookings
                    .AnyAsync(b => b.EventId == id && b.BookingDate >= DateTime.Today);

                if (hasActiveBookings)
                {
                    TempData["ErrorMessage"] = "Cannot delete this event because it has active bookings.";
                    return RedirectToAction(nameof(Index));
                }

                var eventItem = await _context.Events.FindAsync(id);
                if (eventItem != null)
                {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event");
                TempData["ErrorMessage"] = "Error deleting event. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}