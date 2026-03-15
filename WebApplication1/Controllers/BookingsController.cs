using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class BookingsController : Controller
{
    private readonly AppDbContext _context;

    public BookingsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Bookings
    public async Task<IActionResult> Index(string? search)
    {
        ViewBag.Search = search;

        var query = _context.BookingDetails.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            if (int.TryParse(search, out int bookingId))
                query = query.Where(b => b.BookingId == bookingId);
            else
                query = query.Where(b => b.EventName.Contains(search));
        }

        var results = await query.ToListAsync();
        return View(results);
    }

    // GET: Bookings/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var booking = await _context.Bookings
            .Include(b => b.Event)
            .Include(b => b.Venue)
            .FirstOrDefaultAsync(b => b.BookingId == id);
        if (booking == null) return NotFound();

        return View(booking);
    }

    // GET: Bookings/Create
    public IActionResult Create()
    {
        ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName");
        ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName");
        return View();
    }

    // POST: Bookings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EventId,VenueId,BookingDate")] Booking booking)
    {
        if (ModelState.IsValid)
        {
            // Double-booking check: same venue on the same date
            var conflict = await _context.Bookings.AnyAsync(b =>
                b.VenueId == booking.VenueId &&
                b.BookingDate.Date == booking.BookingDate.Date);

            if (conflict)
            {
                ModelState.AddModelError(string.Empty, "This venue is already booked on the selected date. Please choose a different date or venue.");
                ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }

            _context.Add(booking);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Booking created successfully.";
            return RedirectToAction(nameof(Index));
        }
        ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
        ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
        return View(booking);
    }

    // GET: Bookings/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();

        ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
        ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
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
            // Double-booking check (exclude self)
            var conflict = await _context.Bookings.AnyAsync(b =>
                b.VenueId == booking.VenueId &&
                b.BookingDate.Date == booking.BookingDate.Date &&
                b.BookingId != booking.BookingId);

            if (conflict)
            {
                ModelState.AddModelError(string.Empty, "This venue is already booked on the selected date. Please choose a different date or venue.");
                ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }

            try
            {
                _context.Update(booking);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(booking.BookingId)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
        ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
        return View(booking);
    }

    // GET: Bookings/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var booking = await _context.Bookings
            .Include(b => b.Event)
            .Include(b => b.Venue)
            .FirstOrDefaultAsync(b => b.BookingId == id);
        if (booking == null) return NotFound();

        return View(booking);
    }

    // POST: Bookings/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking != null)
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Booking removed successfully.";
        }
        return RedirectToAction(nameof(Index));
    }

    private bool BookingExists(int id) =>
        _context.Bookings.Any(b => b.BookingId == id);
}
