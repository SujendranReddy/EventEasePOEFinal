using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly EventEaseContext _context;
        private readonly string _blobConnectionString;
        private readonly string _blobContainerName;

        public VenuesController(EventEaseContext context, IConfiguration configuration)
        {
            _context = context;
            _blobConnectionString = configuration.GetValue<string>("AzureBlobStorage:ConnectionString");
            _blobContainerName = configuration.GetValue<string>("AzureBlobStorage:ContainerName");
        }

        // GET: Venues
        public async Task<IActionResult> Index()
        {
            return View(await _context.Venue.ToListAsync());
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue
                                .AsNoTracking()
                                .FirstOrDefaultAsync(v => v.VenueId == id);
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
        public async Task<IActionResult> Create(
            [Bind("VenueName,Location,Capacity,Availability")] Venue venue,
            IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                // Duplicate check
                bool duplicateExists = await _context.Venue
                    .AnyAsync(v =>
                        v.VenueName.ToLower() == venue.VenueName.ToLower() &&
                        v.Location.ToLower() == venue.Location.ToLower());

                if (duplicateExists)
                {
                    ModelState.AddModelError(string.Empty, "A venue with the same name and location already exists.");
                    return View(venue);
                }

                // Upload image if provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    if (!imageFile.ContentType.StartsWith("image/"))
                    {
                        ModelState.AddModelError("ImageFile", "Please upload a valid image file.");
                        return View(venue);
                    }
                    venue.ImageURL = await UploadImageToBlob(imageFile);
                }

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

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // POST: Venues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("VenueId,VenueName,Location,Capacity,Availability,ImageUrl")] Venue venue,
            IFormFile imageFile,
            bool deleteImage)
        {
            if (id != venue.VenueId) return NotFound();

            var existing = await _context.Venue
                                .AsNoTracking()
                                .FirstOrDefaultAsync(v => v.VenueId == id);
            if (existing == null) return NotFound();

            if (ModelState.IsValid)
            {
                // Duplicate check excluding self
                bool duplicateExists = await _context.Venue
                    .AnyAsync(v =>
                        v.VenueId != venue.VenueId &&
                        v.VenueName.ToLower() == venue.VenueName.ToLower() &&
                        v.Location.ToLower() == venue.Location.ToLower());

                if (duplicateExists)
                {
                    ModelState.AddModelError(string.Empty, "A venue with the same name and location already exists.");
                    return View(venue);
                }

                // Handle image deletion
                if (deleteImage && !string.IsNullOrEmpty(existing.ImageURL))
                {
                    await DeleteImageFromBlob(existing.ImageURL);
                    venue.ImageURL = null;
                }
                else
                {
                    venue.ImageURL = existing.ImageURL;
                }

                // Handle new upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(existing.ImageURL))
                        await DeleteImageFromBlob(existing.ImageURL);

                    venue.ImageURL = await UploadImageToBlob(imageFile);
                }

                try
                {
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
                        return NotFound();
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

            var venue = await _context.Venue
                                .AsNoTracking()
                                .FirstOrDefaultAsync(v => v.VenueId == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            // Prevent deletion if future bookings exist
            bool hasActive = await _context.Booking
                .AnyAsync(b => b.VenueId == id && b.BookingDate > DateTime.Now);
            if (hasActive)
            {
                ModelState.AddModelError(string.Empty, "Cannot delete: active bookings exist.");
                return View(venue);
            }

            if (!string.IsNullOrEmpty(venue.ImageURL))
                await DeleteImageFromBlob(venue.ImageURL);

            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id) =>
            _context.Venue.Any(e => e.VenueId == id);

        private async Task<string> UploadImageToBlob(IFormFile file)
        {
            var blobService = new BlobServiceClient(_blobConnectionString);
            var container = blobService.GetBlobContainerClient(_blobContainerName);
            await container.CreateIfNotExistsAsync();
            await container.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            var name = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var client = container.GetBlobClient(name);

            using var stream = file.OpenReadStream();
            await client.UploadAsync(stream, overwrite: true);

            return client.Uri.ToString();
        }

        private async Task DeleteImageFromBlob(string imageUrl)
        {
            var blobService = new BlobServiceClient(_blobConnectionString);
            var container = blobService.GetBlobContainerClient(_blobContainerName);
            var fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
            var client = container.GetBlobClient(fileName);

            await client.DeleteIfExistsAsync();
        }
    }
}
