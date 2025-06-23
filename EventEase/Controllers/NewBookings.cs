using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;

namespace EventEase.Controllers
{
    public class NewBookingsController : Controller
    {
        private readonly EventEaseContext _context;

        public NewBookingsController(EventEaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string searchQuery,
            int? eventTypeId,
            bool? venueAvailable,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            var bookings = _context.Booking
                .Include(b => b.Event)
                    .ThenInclude(e => e.EventType)
                .Include(b => b.Venue)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                bookings = bookings.Where(b =>
                    b.BookingId.ToString().Contains(searchQuery) ||
                    b.Event.EventName.Contains(searchQuery));
            }

            if (eventTypeId.HasValue)
            {
                bookings = bookings.Where(b =>
                    b.Event.EventTypeId == eventTypeId.Value);
            }

            if (venueAvailable.HasValue)
            {
                bookings = bookings.Where(b =>
                    b.Venue.Availability == venueAvailable.Value);
            }

            if (dateFrom.HasValue)
            {
                bookings = bookings.Where(b =>
                    b.Event.EventDate >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                bookings = bookings.Where(b =>
                    b.Event.EventDate <= dateTo.Value);
            }

            var types = await _context.EventType
                .OrderBy(t => t.EventTypeName)
                .ToListAsync();

            ViewData["EventTypeId"] = new SelectList(types, "EventTypeId", "EventTypeName", eventTypeId);
            ViewData["CurrentSearch"] = searchQuery;
            ViewData["DateFrom"] = dateFrom?.ToString("yyyy-MM-dd");
            ViewData["DateTo"] = dateTo?.ToString("yyyy-MM-dd");
            ViewData["CurrentAvailable"] = venueAvailable?.ToString();

            return View(await bookings.ToListAsync());
        }
    }
}
