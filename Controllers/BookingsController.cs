using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ST10296771_CLDV7311_POE.Controllers;
using ST10296771_CLDV7311_POE.Data;
using ST10296771_CLDV7311_POE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ST10296771_CLDV7311_POE.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var bookings = _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .Include(b => b.User);

            return View(await bookings.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName");
            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName");
            ViewBag.UserId = new SelectList(_context.Users, "UserId", "Username");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                // CHECK FOR DOUBLE BOOKING
                bool isAlreadyBooked = await _context.Bookings
                    .AnyAsync(b => b.VenueId == booking.VenueId
                                   && b.BookingDate.Date == booking.BookingDate.Date);

                if (isAlreadyBooked)
                {
                    ModelState.AddModelError("VenueId", "This venue is already booked on the selected date.");

                    // Re-populate dropdowns
                    ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                    ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                    ViewBag.UserId = new SelectList(_context.Users, "UserId", "Username", booking.CreatedBy);

                    return View(booking);
                }

                booking.BookingDate = DateTime.Now;

                _context.Add(booking);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            ViewBag.UserId = new SelectList(_context.Users, "UserId", "Username", booking.CreatedBy);

            return View(booking);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            ViewBag.UserId = new SelectList(_context.Users, "UserId", "Username", booking.CreatedBy);

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // CHECK FOR DOUBLE BOOKING (excluding current booking)
                    bool isAlreadyBooked = await _context.Bookings
                        .AnyAsync(b => b.VenueId == booking.VenueId
                                       && b.BookingDate.Date == booking.BookingDate.Date
                                       && b.BookingId != booking.BookingId);

                    if (isAlreadyBooked)
                    {
                        ModelState.AddModelError("VenueId", "This venue is already booked on the selected date.");

                        ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                        ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                        ViewBag.UserId = new SelectList(_context.Users, "UserId", "Username", booking.CreatedBy);

                        return View(booking);
                    }

                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            ViewBag.UserId = new SelectList(_context.Users, "UserId", "Username", booking.CreatedBy);

            return View(booking);
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int venueId, DateTime bookingDate)
        {
            var isBooked = await _context.Bookings
                .AnyAsync(b => b.VenueId == venueId && b.BookingDate.Date == bookingDate.Date);

            return Json(new { isAvailable = !isBooked });
        }
    }
}