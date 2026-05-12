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
                        ImageFileName = v.ImageFileName ?? string.Empty,
                        ImageContentType = v.ImageContentType ?? string.Empty
                    })
                    .ToListAsync();

                return View(venues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading venues");
                TempData["ErrorMessage"] = "Unable to load venues. Please try again later.";
                return View(new List<Venue>());
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var venue = await _context.Venues
                    .FirstOrDefaultAsync(v => v.VenueId == id);

                if (venue == null) return NotFound();

                // Safely handle NULL values
                if (venue.VenueName == null) venue.VenueName = "Unknown Venue";
                if (venue.Location == null) venue.Location = "Unknown Location";
                if (venue.ContactPhone == null) venue.ContactPhone = "Not Provided";
                if (venue.ContactEmail == null) venue.ContactEmail = "Not Provided";
                if (venue.ImageFileName == null) venue.ImageFileName = string.Empty;
                if (venue.ImageContentType == null) venue.ImageContentType = string.Empty;

                return View(venue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading venue details");
                TempData["ErrorMessage"] = "Unable to load venue details.";
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
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingVenue.ImageFileName))
                        {
                            await _blobStorageService.DeleteImageAsync(existingVenue.ImageFileName);
                        }

                        // Upload new image
                        var fileName = await _blobStorageService.UploadImageAsync(imageFile, imageFile.FileName);
                        venue.ImageFileName = fileName;
                        venue.ImageContentType = imageFile.ContentType;
                    }
                    else
                    {
                        // Keep existing image
                        venue.ImageFileName = existingVenue.ImageFileName;
                        venue.ImageContentType = existingVenue.ImageContentType;
                    }

                    // Ensure no NULL values
                    if (string.IsNullOrEmpty(venue.VenueName)) venue.VenueName = "Unknown Venue";
                    if (string.IsNullOrEmpty(venue.Location)) venue.Location = "Unknown Location";
                    if (string.IsNullOrEmpty(venue.ContactPhone)) venue.ContactPhone = "Not Provided";
                    if (string.IsNullOrEmpty(venue.ContactEmail)) venue.ContactEmail = "Not Provided";

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
                // CHECK FOR ACTIVE BOOKINGS
                var hasActiveBookings = await _context.Bookings
                    .AnyAsync(b => b.VenueId == id && b.BookingDate >= DateTime.Today);

                if (hasActiveBookings)
                {
                    TempData["ErrorMessage"] = "Cannot delete this venue because it has active bookings (today or future).";
                    return RedirectToAction(nameof(Index));
                }

                var venue = await _context.Venues.FindAsync(id);
                if (venue != null)
                {
                    // Delete image from Azure Blob Storage
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
                _logger.LogError(ex, "Error deleting venue");
                TempData["ErrorMessage"] = "Error deleting venue. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Add this action to fix NULL values
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

                    if (string.IsNullOrEmpty(venue.VenueName))
                    {
                        venue.VenueName = "Unknown Venue";
                        updated = true;
                    }
                    if (string.IsNullOrEmpty(venue.Location))
                    {
                        venue.Location = "Unknown Location";
                        updated = true;
                    }
                    if (string.IsNullOrEmpty(venue.ContactPhone))
                    {
                        venue.ContactPhone = "Not Provided";
                        updated = true;
                    }
                    if (string.IsNullOrEmpty(venue.ContactEmail))
                    {
                        venue.ContactEmail = "Not Provided";
                        updated = true;
                    }
                    if (venue.ImageFileName == null)
                    {
                        venue.ImageFileName = string.Empty;
                        updated = true;
                    }
                    if (venue.ImageContentType == null)
                    {
                        venue.ImageContentType = string.Empty;
                        updated = true;
                    }
                    if (venue.Capacity == 0)
                    {
                        venue.Capacity = 100; // Default capacity
                        updated = true;
                    }

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
