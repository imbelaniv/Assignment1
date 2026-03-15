using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class VenuesController : Controller
{
    private readonly AppDbContext _context;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public VenuesController(AppDbContext context, BlobServiceClient blobServiceClient, IConfiguration configuration)
    {
        _context = context;
        _blobServiceClient = blobServiceClient;
        _containerName = configuration["AzureBlob:ContainerName"] ?? "venue-images";
    }

    // GET: Venues
    public async Task<IActionResult> Index()
    {
        return View(await _context.Venues.ToListAsync());
    }

    // GET: Venues/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var venue = await _context.Venues.FirstOrDefaultAsync(v => v.VenueId == id);
        if (venue == null) return NotFound();

        return View(venue);
    }

    // GET: Venues/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Venues/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("VenueName,Location,Capacity")] Venue venue, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            venue.ImageUrl = await UploadImageAsync(imageFile);
            _context.Add(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(venue);
    }

    // GET: Venues/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var venue = await _context.Venues.FindAsync(id);
        if (venue == null) return NotFound();

        return View(venue);
    }

    // POST: Venues/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity,ImageUrl")] Venue venue, IFormFile? imageFile)
    {
        if (id != venue.VenueId) return NotFound();

        if (ModelState.IsValid)
        {
            if (imageFile != null)
                venue.ImageUrl = await UploadImageAsync(imageFile);

            try
            {
                _context.Update(venue);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VenueExists(venue.VenueId)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(venue);
    }

    // GET: Venues/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var venue = await _context.Venues.FirstOrDefaultAsync(v => v.VenueId == id);
        if (venue == null) return NotFound();

        return View(venue);
    }

    // POST: Venues/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (_context.Bookings.Any(b => b.VenueId == id))
        {
            TempData["ErrorMessage"] = "Cannot delete this venue because it has active bookings.";
            return RedirectToAction(nameof(Index));
        }

        var venue = await _context.Venues.FindAsync(id);
        if (venue != null)
        {
            // Delete blob image if one exists
            if (!string.IsNullOrEmpty(venue.ImageUrl))
            {
                try
                {
                    var container = _blobServiceClient.GetBlobContainerClient(_containerName);
                    var uri = new Uri(venue.ImageUrl);
                    var blobName = Path.GetFileName(uri.LocalPath);
                    await container.GetBlobClient(blobName).DeleteIfExistsAsync();
                }
                catch { /* ignore blob deletion errors */ }
            }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Venue deleted successfully.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> UploadImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        // Use placeholder if blob storage is not configured
        if (_blobServiceClient.Uri.Host == "placeholder.blob.core.windows.net")
            return "https://placehold.co/400x300?text=Venue+Image";

        var container = _blobServiceClient.GetBlobContainerClient(_containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var blob = container.GetBlobClient(blobName);

        await blob.UploadAsync(file.OpenReadStream(), new BlobHttpHeaders
        {
            ContentType = file.ContentType
        });

        return blob.Uri.ToString();
    }

    private bool VenueExists(int id) =>
        _context.Venues.Any(v => v.VenueId == id);
}
