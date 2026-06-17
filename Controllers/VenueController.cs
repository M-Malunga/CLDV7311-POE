using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10296771_CLDV7311_POE.Data;
using ST10296771_CLDV7311_POE.Models;
using ST10296771_CLDV7311_POE.Services;

namespace ST10296771_CLDV7311_POE.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<VenuesController> _logger;

        public VenuesController(
            ApplicationDbContext context,
            IBlobStorageService blobStorageService,
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            ILogger<VenuesController> logger)
        {
            _context = context;
            _blobStorageService = blobStorageService;
            _webHostEnvironment = webHostEnvironment;
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
                var venues = await _context.Venues
                    .Select(v => new Venue
                    {
                        VenueId = v.VenueId,
                        VenueName = v.VenueName ?? "Unknown Venue",
                        Location = v.Location ?? "Unknown Location",
                        Capacity = v.Capacity,
                        ContactPhone = v.ContactPhone ?? "Not Provided",
                        ContactEmail = v.ContactEmail ?? "Not Provided",
                        ImageFileName = v.ImageFileName ?? "",
                        ImageContentType = v.ImageContentType ?? "",
                        IsAvailable = v.IsAvailable,
                        OperatingHours = v.OperatingHours ?? "Not specified",
                        IsIndoor = v.IsIndoor,
                        HasParking = v.HasParking,
                        IsWheelchairAccessible = v.IsWheelchairAccessible
                    })
                    .ToListAsync();

                return View(venues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading venues");
                TempData["ErrorMessage"] = "Error loading venues. Please try again.";
                return View(new List<Venue>());
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
                // Use projection to handle NULL values safely
                var venue = await _context.Venues
                    .Where(v => v.VenueId == id)
                    .Select(v => new Venue
                    {
                        VenueId = v.VenueId,
                        VenueName = v.VenueName ?? "Unknown Venue",
                        Location = v.Location ?? "Unknown Location",
                        Capacity = v.Capacity,
                        ContactPhone = v.ContactPhone ?? "Not Provided",
                        ContactEmail = v.ContactEmail ?? "Not Provided",
                        ImageFileName = v.ImageFileName ?? "",
                        ImageContentType = v.ImageContentType ?? "",
                        IsAvailable = v.IsAvailable,
                        AvailableFrom = v.AvailableFrom,
                        AvailableTo = v.AvailableTo,
                        OperatingHours = v.OperatingHours ?? "9:00 AM - 9:00 PM",
                        DaysAvailable = v.DaysAvailable ?? "Monday - Sunday",
                        Amenities = v.Amenities ?? "No amenities listed",
                        IsIndoor = v.IsIndoor,
                        HasParking = v.HasParking,
                        IsWheelchairAccessible = v.IsWheelchairAccessible
                    })
                    .FirstOrDefaultAsync();

                if (venue == null)
                {
                    return NotFound();
                }

                return View(venue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading venue details for ID: {id}");
                TempData["ErrorMessage"] = "Unable to load venue details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to create venues.";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue, IFormFile imageFile)
        {
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to create venues.";
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
                        venue.ImageFileName = fileName;
                        venue.ImageContentType = imageFile.ContentType;
                    }

                    // Ensure no NULL values
                    if (string.IsNullOrEmpty(venue.VenueName)) venue.VenueName = "Unknown Venue";
                    if (string.IsNullOrEmpty(venue.Location)) venue.Location = "Unknown Location";
                    if (string.IsNullOrEmpty(venue.ContactPhone)) venue.ContactPhone = "Not Provided";
                    if (string.IsNullOrEmpty(venue.ContactEmail)) venue.ContactEmail = "Not Provided";
                    if (string.IsNullOrEmpty(venue.OperatingHours)) venue.OperatingHours = "9:00 AM - 9:00 PM";
                    if (string.IsNullOrEmpty(venue.DaysAvailable)) venue.DaysAvailable = "Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday";
                    if (string.IsNullOrEmpty(venue.Amenities)) venue.Amenities = "";

                    // Set defaults for boolean fields
                    venue.IsAvailable = true;
                    venue.IsIndoor = true;
                    venue.HasParking = true;
                    venue.IsWheelchairAccessible = true;

                    _context.Add(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Venue created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating venue");
                    ModelState.AddModelError("", "Error creating venue. Please try again.");
                }
            }
            return View(venue);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to edit venues.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return NotFound();

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue, IFormFile imageFile)
        {
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to edit venues.";
                return RedirectToAction("Index", "Home");
            }

            if (id != venue.VenueId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingVenue = await _context.Venues.FindAsync(id);
                    if (existingVenue == null) return NotFound();

                    // Handle image update
                    if (imageFile != null && _blobStorageService.IsValidImage(imageFile))
                    {
                        if (!string.IsNullOrEmpty(existingVenue.ImageFileName))
                        {
                            await _blobStorageService.DeleteImageAsync(existingVenue.ImageFileName);
                        }
                        var fileName = await _blobStorageService.UploadImageAsync(imageFile, imageFile.FileName);
                        venue.ImageFileName = fileName;
                        venue.ImageContentType = imageFile.ContentType;
                    }
                    else
                    {
                        venue.ImageFileName = existingVenue.ImageFileName;
                        venue.ImageContentType = existingVenue.ImageContentType;
                    }

                    // Ensure no NULL values
                    if (string.IsNullOrEmpty(venue.VenueName)) venue.VenueName = "Unknown Venue";
                    if (string.IsNullOrEmpty(venue.Location)) venue.Location = "Unknown Location";
                    if (string.IsNullOrEmpty(venue.ContactPhone)) venue.ContactPhone = "Not Provided";
                    if (string.IsNullOrEmpty(venue.ContactEmail)) venue.ContactEmail = "Not Provided";
                    if (string.IsNullOrEmpty(venue.OperatingHours)) venue.OperatingHours = "9:00 AM - 9:00 PM";
                    if (string.IsNullOrEmpty(venue.DaysAvailable)) venue.DaysAvailable = "Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday";

                    _context.Entry(existingVenue).CurrentValues.SetValues(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Venue updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to delete venues.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return NotFound();

            var venue = await _context.Venues
                .FirstOrDefaultAsync(v => v.VenueId == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to delete venues.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var hasActiveBookings = await _context.Bookings
                    .AnyAsync(b => b.VenueId == id && b.BookingDate >= DateTime.Today);

                if (hasActiveBookings)
                {
                    TempData["ErrorMessage"] = "Cannot delete this venue because it has active bookings.";
                    return RedirectToAction(nameof(Index));
                }

                var venue = await _context.Venues.FindAsync(id);
                if (venue != null)
                {
                    if (!string.IsNullOrEmpty(venue.ImageFileName))
                    {
                        await _blobStorageService.DeleteImageAsync(venue.ImageFileName);
                    }
                    _context.Venues.Remove(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Venue deleted successfully.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting venue ID: {id}");
                TempData["ErrorMessage"] = "Error deleting venue. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Venues/FixNullVenues - Temporary fix for NULL values
        [HttpGet]
        public async Task<IActionResult> FixNullVenues()
        {
            try
            {
                var venues = await _context.Venues.ToListAsync();
                int updatedCount = 0;

                foreach (var venue in venues)
                {
                    bool updated = false;

                    if (string.IsNullOrEmpty(venue.VenueName)) { venue.VenueName = "Unknown Venue"; updated = true; }
                    if (string.IsNullOrEmpty(venue.Location)) { venue.Location = "Unknown Location"; updated = true; }
                    if (string.IsNullOrEmpty(venue.ContactPhone)) { venue.ContactPhone = "Not Provided"; updated = true; }
                    if (string.IsNullOrEmpty(venue.ContactEmail)) { venue.ContactEmail = "Not Provided"; updated = true; }
                    if (string.IsNullOrEmpty(venue.OperatingHours)) { venue.OperatingHours = "9:00 AM - 9:00 PM"; updated = true; }
                    if (string.IsNullOrEmpty(venue.DaysAvailable)) { venue.DaysAvailable = "Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday"; updated = true; }
                    if (string.IsNullOrEmpty(venue.Amenities)) { venue.Amenities = ""; updated = true; }
                    if (venue.ImageFileName == null) { venue.ImageFileName = ""; updated = true; }
                    if (venue.ImageContentType == null) { venue.ImageContentType = ""; updated = true; }
                    if (venue.Capacity == 0) { venue.Capacity = 100; updated = true; }

                    if (updated) updatedCount++;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Fixed {updatedCount} venues with NULL or invalid values.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing venue data");
                TempData["ErrorMessage"] = "Error fixing venue data.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }
    }
}