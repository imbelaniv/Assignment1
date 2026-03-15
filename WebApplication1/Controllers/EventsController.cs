using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class EventsController : Controller
{
    private readonly AppDbContext _context;

    public EventsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Events
    public async Task<IActionResult> Index()
    {
        var events = await _context.Events
            .Include(e => e.Venue)
            .ToListAsync();
        return View(events);
    }

    // GET: Events/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var ev = await _context.Events
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.EventId == id);
        if (ev == null) return NotFound();

        return View(ev);
    }

    // GET: Events/Create
    public IActionResult Create()
    {
        ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName");
        return View();
    }

    // POST: Events/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EventName,EventDate,Description,VenueId")] Event ev)
    {
        if (ModelState.IsValid)
        {
            _context.Add(ev);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", ev.VenueId);
        return View(ev);
    }

    // GET: Events/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();

        ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", ev.VenueId);
        return View(ev);
    }

    // POST: Events/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDate,Description,VenueId")] Event ev)
    {
        if (id != ev.EventId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(ev);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(ev.EventId)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", ev.VenueId);
        return View(ev);
    }

    // GET: Events/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var ev = await _context.Events
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.EventId == id);
        if (ev == null) return NotFound();

        return View(ev);
    }

    // POST: Events/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (_context.Bookings.Any(b => b.EventId == id))
        {
            TempData["ErrorMessage"] = "Cannot delete this event because it has active bookings.";
            return RedirectToAction(nameof(Index));
        }

        var ev = await _context.Events.FindAsync(id);
        if (ev != null)
        {
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Event deleted successfully.";
        }
        return RedirectToAction(nameof(Index));
    }

    private bool EventExists(int id) =>
        _context.Events.Any(e => e.EventId == id);
}
