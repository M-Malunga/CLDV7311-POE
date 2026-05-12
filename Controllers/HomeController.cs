using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10296771_CLDV7311_POE.Data;
using ST10296771_CLDV7311_POE.Models;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ST10296771_CLDV7311_POE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Debug: Log the counts
                var venueCount = await _context.Venues.CountAsync();
                var eventCount = await _context.Events.CountAsync();

                _logger.LogInformation($"Venue count: {venueCount}, Event count: {eventCount}");

                // Get upcoming events (with null-safe handling)
                var upcomingEvents = await _context.Events
                    .Include(e => e.Venue)
                    .Where(e => e.EventDate >= DateTime.Today)
                    .OrderBy(e => e.EventDate)
                    .Take(6)
                    .Select(e => new
                    {
                        e.EventId,
                        EventName = e.EventName ?? "Untitled Event",
                        EventDate = e.EventDate,
                        Description = e.Description ?? "",
                        VenueName = e.Venue != null ? (e.Venue.VenueName ?? "TBA") : "TBA",
                        e.VenueId,
                        ImageFileName = e.ImageFileName ?? "",
                        ImageContentType = e.ImageContentType ?? ""
                    })
                    .ToListAsync();

                // Get featured venues (with null-safe handling)
                var featuredVenues = await _context.Venues
                    .Take(4)
                    .Select(v => new
                    {
                        v.VenueId,
                        VenueName = v.VenueName ?? "Unknown Venue",
                        Location = v.Location ?? "Unknown Location",
                        Capacity = v.Capacity,
                        ContactPhone = v.ContactPhone ?? "",
                        ImageFileName = v.ImageFileName ?? "",
                        ImageContentType = v.ImageContentType ?? ""
                    })
                    .ToListAsync();

                // Pass data to view using ViewBag
                ViewBag.UpcomingEvents = upcomingEvents;
                ViewBag.FeaturedVenues = featuredVenues;
                ViewBag.VenueCount = venueCount;
                ViewBag.EventCount = eventCount;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                ViewBag.ErrorMessage = $"Unable to load events and venues. Error: {ex.Message}";
                ViewBag.UpcomingEvents = new List<object>();
                ViewBag.FeaturedVenues = new List<object>();
                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}