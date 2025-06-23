// File: Controllers/BookingsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Ease.Controllers
{
    public class BookingsController : Controller
    {
        private readonly EventEaseContext _context;

        public BookingsController(EventEaseContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index(int? venueId, DateTime? startDate, DateTime? endDate)
        {
            var bookings = _context.Booking
                                   .Include(b => b.Event)
                                   .Include(b => b.Venue)
                                   .AsQueryable();

            if (venueId.HasValue)
                bookings = bookings.Where(b => b.VenueId == venueId.Value);

            if (startDate.HasValue)
                bookings = bookings.Where(b => b.BookingDate >= startDate.Value);

            if (endDate.HasValue)
                bookings = bookings.Where(b => b.BookingDate <= endDate.Value);

            // Populate dropdown and filter values via ViewBag
            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName", venueId);
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd") ?? "";

            return View(await bookings.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewBag.Events = new SelectList(_context.Event, "EventId", "EventName");
            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName");
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,EventId,VenueId,BookingDate")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                bool conflict = await _context.Booking
                    .AnyAsync(b => b.VenueId == booking.VenueId
                                 && b.BookingDate == booking.BookingDate);

                if (conflict)
                {
                    ModelState.AddModelError(string.Empty, "A booking for that date and venue already exists.");
                }
                else
                {
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Events = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null) return NotFound();

            ViewBag.Events = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,VenueId,BookingDate")] Booking booking)
        {
            if (id != booking.BookingId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    bool conflict = await _context.Booking
                        .AnyAsync(b => b.VenueId == booking.VenueId
                                     && b.BookingDate == booking.BookingDate
                                     && b.BookingId != booking.BookingId);

                    if (conflict)
                    {
                        ModelState.AddModelError(string.Empty, "A booking for that date and venue already exists.");
                        ViewBag.Events = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
                        ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName", booking.VenueId);
                        return View(booking);
                    }

                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Booking.Any(e => e.BookingId == booking.BookingId))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewBag.Events = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
