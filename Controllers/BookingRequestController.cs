using ST10296771_CLDV7311_POE.Data;
using ST10296771_CLDV7311_POE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ST10296771_CLDV7311_POE.Controllers
{
    public class BookingRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BookingRequestsController> _logger;

        public BookingRequestsController(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<BookingRequestsController> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private bool IsAuthenticated(out string role, out int userId)
        {
            role = _httpContextAccessor.HttpContext.Session.GetString("UserRole") ?? string.Empty;
            userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId") ?? 0;
            return !string.IsNullOrEmpty(role) && userId > 0;
        }

        private bool IsAdminOrEmployee()
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("UserRole") ?? string.Empty;
            return role == "Admin" || role == "Employee";
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                if (!IsAuthenticated(out string role, out int userId))
                    return RedirectToAction("Login", "Account");

                if (role == "Customer")
                {
                    var requests = await _context.BookingRequests
                        .Include(r => r.Venue)
                        .Include(r => r.Customer)
                        .Where(r => r.CustomerId == userId)
                        .OrderByDescending(r => r.EventDate)
                        .Select(r => new 
                        {
                            r.RequestId,
                            r.EventName,
                            r.EventDate,
                            r.ExpectedAttendees,
                            r.RequestDate,
                            r.Status,
                            r.VenueId,
                            VenueName = r.Venue != null ? r.Venue.VenueName : "N/A",
                            CustomerName = r.Customer != null ? r.Customer.Username : "Unknown"
                        })
                        .ToListAsync();

                    var bookingRequests = requests.Select(r => new BookingRequest
                    {
                        RequestId = r.RequestId,
                        EventName = r.EventName ?? "Untitled Event",
                        EventDate = r.EventDate,
                        ExpectedAttendees = r.ExpectedAttendees,
                        RequestDate = r.RequestDate,
                        Status = r.Status ?? "Pending",
                        VenueId = r.VenueId,
                        Venue = new Venue { VenueName = r.VenueName ?? "N/A" }
                    }).ToList();

                    return View("CustomerRequests", bookingRequests);
                }
                else
                {
                    var requests = await _context.BookingRequests
                        .Include(r => r.Customer)
                        .Include(r => r.Venue)
                        .OrderBy(r => r.Status == "Pending" ? 0 : 1)
                        .ThenByDescending(r => r.EventDate)
                        .Select(r => new 
                        {
                            r.RequestId,
                            r.EventName,
                            r.EventDate,
                            r.ExpectedAttendees,
                            r.RequestDate,
                            r.Status,
                            r.VenueId,
                            VenueName = r.Venue != null ? r.Venue.VenueName : "N/A",
                            CustomerName = r.Customer != null ? r.Customer.Username : "Unknown",
                            CustomerEmail = r.Customer != null ? r.Customer.Email : "No email"
                        })
                        .ToListAsync();

                    var bookingRequests = requests.Select(r => new BookingRequest
                    {
                        RequestId = r.RequestId,
                        EventName = r.EventName ?? "Untitled Event",
                        EventDate = r.EventDate,
                        ExpectedAttendees = r.ExpectedAttendees,
                        RequestDate = r.RequestDate,
                        Status = r.Status ?? "Pending",
                        VenueId = r.VenueId,
                        Venue = new Venue { VenueName = r.VenueName ?? "N/A" },
                        Customer = new User { Username = r.CustomerName ?? "Unknown", Email = r.CustomerEmail ?? "No email" }
                    }).ToList();

                    return View("ManageRequests", bookingRequests);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BookingRequests Index");
                TempData["ErrorMessage"] = "Unable to load booking requests. Please try again later.";
                return View("CustomerRequests", new List<BookingRequest>());
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (!IsAuthenticated(out string role, out _))
                    return RedirectToAction("Login", "Account");

                if (id == null) return NotFound();

                var request = await _context.BookingRequests
                    .Include(r => r.Customer)
                    .Include(r => r.Venue)
                    .FirstOrDefaultAsync(r => r.RequestId == id);

                if (request == null) return NotFound();

                // Safely handle NULL values
                if (request.EventName == null) request.EventName = "Untitled Event";
                if (request.Status == null) request.Status = "Pending";
                if (request.Customer != null && request.Customer.Username == null) 
                    request.Customer.Username = "Unknown";

                if (role == "Customer" && request.CustomerId != _httpContextAccessor.HttpContext.Session.GetInt32("UserId"))
                    return Forbid();

                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BookingRequests Details");
                TempData["ErrorMessage"] = "Unable to load request details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string newStatus)
        {
            try
            {
                if (!IsAdminOrEmployee())
                    return Forbid();

                var request = await _context.BookingRequests.FindAsync(id);
                if (request == null)
                    return NotFound();

                if (newStatus != "Pending" && newStatus != "Approved" && newStatus != "Denied")
                {
                    TempData["ErrorMessage"] = "Invalid status value.";
                    return RedirectToAction(nameof(Index));
                }

                if (newStatus == "Approved")
                {
                    bool isBooked = await _context.BookingRequests
                        .AnyAsync(br => br.RequestId != id
                            && br.VenueId == request.VenueId
                            && br.EventDate.Date == request.EventDate.Date
                            && br.Status == "Approved");

                    if (isBooked)
                    {
                        TempData["ErrorMessage"] = "Cannot approve: This venue is already booked for the selected date by another approved request.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                string oldStatus = request.Status ?? "Pending";
                request.Status = newStatus;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Booking request {id} status changed from {oldStatus} to {newStatus}");
                TempData["SuccessMessage"] = $"Booking request status changed to {newStatus} successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing booking request status");
                TempData["ErrorMessage"] = "Unable to change request status.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            return await ChangeStatus(id, "Approved");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decline(int id)
        {
            return await ChangeStatus(id, "Denied");
        }

        // Add this method to fix NULL values in the database
        [HttpGet]
        public async Task<IActionResult> FixNullValues()
        {
            try
            {
                // Fix NULL EventName
                var nullEventNames = await _context.BookingRequests
                    .Where(r => r.EventName == null)
                    .ToListAsync();
                
                foreach (var request in nullEventNames)
                {
                    request.EventName = "Untitled Event";
                }
                
                // Fix NULL Status
                var nullStatus = await _context.BookingRequests
                    .Where(r => r.Status == null)
                    .ToListAsync();
                
                foreach (var request in nullStatus)
                {
                    request.Status = "Pending";
                }
                
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Fixed {nullEventNames.Count + nullStatus.Count} records with NULL values.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing NULL values");
                TempData["ErrorMessage"] = "Error fixing NULL values.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool BookingRequestExists(int id)
        {
            return _context.BookingRequests.Any(e => e.RequestId == id);
        }
    }
}