using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10296771_CLDV7311_POE.Data;
using ST10296771_CLDV7311_POE.Models;
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

        public async Task<IActionResult> Index(string searchTerm = null)
        {
            // Check authorization
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "You do not have permission to view this page.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                IQueryable<BookingDetailsView> query = _context.BookingDetailsViews;

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim();
                    query = query.Where(b =>
                        b.BookingId.ToString().Contains(searchTerm) ||
                        b.EventName.Contains(searchTerm) ||
                        b.CustomerName.Contains(searchTerm) ||
                        b.VenueName.Contains(searchTerm)
                    );
                }

                // Order by upcoming events first
                var bookings = await query
                    .OrderBy(b => b.BookingStatus == "Upcoming" ? 0 : 1)
                    .ThenBy(b => b.EventDate)
                    .ToListAsync();

                ViewBag.CurrentSearch = searchTerm;
                ViewBag.BookingCount = bookings.Count;

                // Statistics for dashboard
                ViewBag.UpcomingCount = bookings.Count(b => b.BookingStatus == "Upcoming");
                ViewBag.TodayCount = bookings.Count(b => b.BookingStatus == "Today");
                ViewBag.CompletedCount = bookings.Count(b => b.BookingStatus == "Completed");

                return View(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking management view");
                TempData["ErrorMessage"] = "Error loading bookings. Please try again.";
                return View(new List<BookingDetailsView>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            if (!IsEmployeeOrAdmin())
                return Json(new { error = "Unauthorized" });

            if (string.IsNullOrWhiteSpace(term))
                return Json(new { results = new List<object>() });

            var results = await _context.BookingDetailsViews
                .Where(b =>
                    b.BookingId.ToString().Contains(term) ||
                    b.EventName.Contains(term) ||
                    b.CustomerName.Contains(term))
                .Take(10)
                .Select(b => new
                {
                    b.BookingId,
                    b.EventName,
                    b.CustomerName,
                    b.EventDate,
                    b.BookingStatus
                })
                .ToListAsync();

            return Json(new { results });
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!IsEmployeeOrAdmin())
            {
                TempData["ErrorMessage"] = "Unauthorized access.";
                return RedirectToAction("Index", "Home");
            }

            var booking = await _context.BookingDetailsViews
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> ExportToCsv()
        {
            if (!IsEmployeeOrAdmin())
                return Forbid();

            var bookings = await _context.BookingDetailsViews
                .OrderBy(b => b.EventDate)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();

            // Add headers
            csv.AppendLine("Booking ID,Event Name,Event Date,Venue,Customer,Status,Expected Attendees,Venue Capacity,Capacity %");

            // Add rows
            foreach (var b in bookings)
            {
                csv.AppendLine($"{b.BookingId},{b.EventName},{b.EventDate:yyyy-MM-dd},{b.VenueName},{b.CustomerName},{b.BookingStatus},{b.ExpectedAttendees},{b.VenueCapacity},{b.CapacityUtilizationPercent}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"Bookings_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
    }
}