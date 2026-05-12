using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10296771_CLDV7311_POE.Controllers;
using ST10296771_CLDV7311_POE.Data;

namespace ST10296771_CLDV7311_POE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalVenues = await _context.Venues.CountAsync();
            ViewBag.TotalEvents = await _context.Events.CountAsync();

            var recentBookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .Include(b => b.User)
                .OrderByDescending(b => b.BookingDate)
                .Take(5)
                .ToListAsync();

            return View(recentBookings);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
