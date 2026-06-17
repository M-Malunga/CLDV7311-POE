using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10296771_CLDV7311_POE.Data;
using ST10296771_CLDV7311_POE.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ST10296771_CLDV7311_POE.Controllers
{
    public class BookingManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BookingManagementController> _logger;

        public BookingManagementController(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<BookingManagementController> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private bool IsEmployeeOrAdmin()
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("UserRole") ?? string.Empty;
            return role == "Employee" || role == "Admin";
        }

        // GET: BookingManagement/AdvancedSearch
        public async Task<IActionResult> AdvancedSearch(AdvancedBookingSearchViewModel searchModel)
        {
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Start with base query
                var query = _context.Bookings
                    .Include(b => b.Event)
                        .ThenInclude(e => e.EventType)
                    .Include(b => b.Venue)
                    .Include(b => b.User)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchModel.SearchTerm))
                {
                    var term = searchModel.SearchTerm.Trim();
                    query = query.Where(b =>
                        b.BookingId.ToString().Contains(term) ||
                        (b.Event != null && b.Event.EventName.Contains(term)) ||
                        (b.Venue != null && b.Venue.VenueName.Contains(term)) ||
                        (b.User != null && b.User.Username.Contains(term))
                    );
                }

                // Filter by Event Type
                if (searchModel.EventTypeId.HasValue && searchModel.EventTypeId > 0)
                {
                    query = query.Where(b => b.Event != null && b.Event.EventTypeId == searchModel.EventTypeId.Value);
                }

                // Filter by Venue Availability
                if (!string.IsNullOrEmpty(searchModel.VenueAvailability))
                {
                    if (searchModel.VenueAvailability == "Available")
                    {
                        query = query.Where(b => b.Venue != null && b.Venue.IsAvailable == true);
                    }
                    else if (searchModel.VenueAvailability == "Unavailable")
                    {
                        query = query.Where(b => b.Venue != null && b.Venue.IsAvailable == false);
                    }
                }

                // Filter by Date Range
                if (searchModel.DateFrom.HasValue)
                {
                    query = query.Where(b => b.Event != null && b.Event.EventDate >= searchModel.DateFrom.Value);
                }
                if (searchModel.DateTo.HasValue)
                {
                    query = query.Where(b => b.Event != null && b.Event.EventDate <= searchModel.DateTo.Value);
                }

                // Filter by Capacity
                if (searchModel.MinCapacity.HasValue)
                {
                    query = query.Where(b => b.Event != null && b.Event.ExpectedAttendees >= searchModel.MinCapacity.Value);
                }
                if (searchModel.MaxCapacity.HasValue)
                {
                    query = query.Where(b => b.Event != null && b.Event.ExpectedAttendees <= searchModel.MaxCapacity.Value);
                }

                // Filter by Venue amenities
                if (searchModel.IsIndoor.HasValue)
                {
                    query = query.Where(b => b.Venue != null && b.Venue.IsIndoor == searchModel.IsIndoor.Value);
                }
                if (searchModel.HasParking.HasValue)
                {
                    query = query.Where(b => b.Venue != null && b.Venue.HasParking == searchModel.HasParking.Value);
                }
                if (searchModel.IsWheelchairAccessible.HasValue)
                {
                    query = query.Where(b => b.Venue != null && b.Venue.IsWheelchairAccessible == searchModel.IsWheelchairAccessible.Value);
                }

                // Filter by Status
                if (!string.IsNullOrEmpty(searchModel.Status))
                {
                    var today = DateTime.Today;
                    switch (searchModel.Status.ToLower())
                    {
                        case "upcoming":
                            query = query.Where(b => b.Event != null && b.Event.EventDate > today);
                            break;
                        case "today":
                            query = query.Where(b => b.Event != null && b.Event.EventDate.Date == today);
                            break;
                        case "past":
                            query = query.Where(b => b.Event != null && b.Event.EventDate < today);
                            break;
                    }
                }

                // Apply sorting
                query = searchModel.SortBy?.ToLower() switch
                {
                    "eventdate" => query.OrderBy(b => b.Event != null ? b.Event.EventDate : DateTime.MaxValue),
                    "eventdatedesc" => query.OrderByDescending(b => b.Event != null ? b.Event.EventDate : DateTime.MinValue),
                    "venue" => query.OrderBy(b => b.Venue != null ? b.Venue.VenueName : ""),
                    "capacity" => query.OrderByDescending(b => b.Event != null ? b.Event.ExpectedAttendees : 0),
                    _ => query.OrderByDescending(b => b.BookingDate)
                };

                var bookings = await query.ToListAsync();

                // Get filter data for dropdowns
                ViewBag.EventTypes = await _context.EventTypes
                    .Where(et => et.IsActive)
                    .OrderBy(et => et.DisplayOrder)
                    .ToListAsync();

                ViewBag.CurrentSearch = searchModel;
                ViewBag.TotalCount = bookings.Count;

                return View(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in advanced search");
                TempData["ErrorMessage"] = "Error performing search. Please try again.";
                ViewBag.EventTypes = await _context.EventTypes.ToListAsync();
                return View(new List<Booking>());
            }
        }

        // GET: BookingManagement/AdvancedSearch (for initial page load)
        public async Task<IActionResult> AdvancedSearchView()
        {
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.EventTypes = await _context.EventTypes
                .Where(et => et.IsActive)
                .OrderBy(et => et.DisplayOrder)
                .ToListAsync();

            return View("AdvancedSearch", new AdvancedBookingSearchViewModel());
        }

        // GET: BookingManagement/GetFilterOptions (AJAX for dynamic filters)
        [HttpGet]
        public async Task<IActionResult> GetFilterOptions()
        {
            var eventTypes = await _context.EventTypes
                .Where(et => et.IsActive)
                .Select(et => new { et.EventTypeId, et.CategoryName, et.IconClass })
                .ToListAsync();

            var venueAmenities = new
            {
                Indoor = await _context.Venues.Select(v => v.IsIndoor).Distinct().ToListAsync(),
                Parking = await _context.Venues.Select(v => v.HasParking).Distinct().ToListAsync(),
                Wheelchair = await _context.Venues.Select(v => v.IsWheelchairAccessible).Distinct().ToListAsync()
            };

            return Json(new { eventTypes, venueAmenities });
        }

        [HttpGet]
        public async Task<IActionResult> ExportFilteredResults(string searchTerm, int? eventTypeId, DateTime? dateFrom, DateTime? dateTo)
        {
            if (!IsEmployeeOrAdmin())
                return Forbid();

            var query = _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e.EventType)
                .Include(b => b.Venue)
                .Include(b => b.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b =>
                    b.BookingId.ToString().Contains(searchTerm) ||
                    (b.Event != null && b.Event.EventName.Contains(searchTerm)));
            }

            if (eventTypeId.HasValue && eventTypeId > 0)
            {
                query = query.Where(b => b.Event != null && b.Event.EventTypeId == eventTypeId);
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(b => b.Event != null && b.Event.EventDate >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(b => b.Event != null && b.Event.EventDate <= dateTo.Value);
            }

            var bookings = await query.ToListAsync();

            // Generate CSV
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Booking ID,Event Name,Event Type,Event Date,Venue,Venue Available,Customer,Booking Date,Status");

            foreach (var b in bookings)
            {
                var status = b.Event != null && b.Event.EventDate < DateTime.Today ? "Completed" :
                            (b.Event != null && b.Event.EventDate.Date == DateTime.Today) ? "Today" : "Upcoming";

                csv.AppendLine($"{b.BookingId},\"{b.Event?.EventName}\",\"{b.Event?.EventType?.CategoryName}\",{b.Event?.EventDate:yyyy-MM-dd},\"{b.Venue?.VenueName}\",{b.Venue?.IsAvailable},\"{b.User?.Username}\",{b.BookingDate:yyyy-MM-dd},{status}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"BookingReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
    }
}