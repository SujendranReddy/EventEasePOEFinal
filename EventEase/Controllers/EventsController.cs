using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventEaseContext _context;

        public EventsController(EventEaseContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var eventsWithTypes = await _context.Event
                .Include(e => e.EventType)
                .ToListAsync();
            return View(eventsWithTypes);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewData["EventTypeId"] = new SelectList(_context.EventType, "EventTypeId", "EventTypeName");
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,EventName,EventDate,Description,EventTypeId")] Event @event)
        {
            if (ModelState.IsValid)
            {
                var normalizedName = @event.EventName?.Trim().ToLowerInvariant();
                var eventDate = @event.EventDate;

                bool duplicateExists = await _context.Event
                    .AnyAsync(e =>
                        e.EventName.ToLower().Trim() == normalizedName &&
                        e.EventDate == eventDate);

                if (duplicateExists)
                {
                    ModelState.AddModelError(string.Empty, "An event with the same name and date already exists.");
                    ViewData["EventTypeId"] = new SelectList(_context.EventType, "EventTypeId", "EventTypeName", @event.EventTypeId);
                    return View(@event);
                }

                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventTypeId"] = new SelectList(_context.EventType, "EventTypeId", "EventTypeName", @event.EventTypeId);
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event.FindAsync(id);
            if (@event == null) return NotFound();

            ViewData["EventTypeId"] = new SelectList(_context.EventType, "EventTypeId", "EventTypeName", @event.EventTypeId);
            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDate,Description,EventTypeId")] Event @event)
        {
            if (id != @event.EventId) return NotFound();

            if (ModelState.IsValid)
            {
                var normalizedName = @event.EventName?.Trim().ToLowerInvariant();
                var eventDate = @event.EventDate;

                bool duplicateExists = await _context.Event
                    .AnyAsync(e =>
                        e.EventId != @event.EventId &&
                        e.EventName.ToLower().Trim() == normalizedName &&
                        e.EventDate == eventDate);

                if (duplicateExists)
                {
                    ModelState.AddModelError(string.Empty, "An event with the same name and date already exists.");
                    ViewData["EventTypeId"] = new SelectList(_context.EventType, "EventTypeId", "EventTypeName", @event.EventTypeId);
                    return View(@event);
                }

                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventTypeId"] = new SelectList(_context.EventType, "EventTypeId", "EventTypeName", @event.EventTypeId);
            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventToDelete = await _context.Event.FirstOrDefaultAsync(e => e.EventId == id);
            if (eventToDelete == null) return NotFound();

            bool hasActiveBookings = await _context.Booking
                .AnyAsync(b => b.EventId == id && b.BookingDate > DateTime.Now);

            if (hasActiveBookings)
            {
                ModelState.AddModelError(string.Empty, "This event cannot be deleted because there are active bookings.");
                return View(eventToDelete);
            }

            _context.Event.Remove(eventToDelete);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.EventId == id);
        }
    }
}
