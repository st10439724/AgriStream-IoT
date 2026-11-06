using AgriStream.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriStream.Web.Controllers
{
    public class DroneController : Controller
    {
        private static readonly string[] AllowedTypes = { ".jpg", ".jpeg", ".png" };
        private const long MaxBytes = 20 * 1024 * 1024;

        private readonly BlobService _blobs;
        private readonly ILogger<DroneController> _logger;

        public DroneController(BlobService blobs, ILogger<DroneController> logger)
        {
            _blobs = blobs;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Upload() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile? file, string fieldId)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                fieldId = "FIELD-A";

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Choose an image first.");
                return View();
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedTypes.Contains(ext) || file.Length > MaxBytes)
            {
                ModelState.AddModelError(string.Empty, "Only JPG or PNG files up to 20 MB are allowed.");
                return View();
            }

            try
            {
                var safeName = Path.GetFileName(file.FileName);
                var blobName = $"{fieldId}/{DateTime.UtcNow:yyyyMMddHHmmss}_{safeName}";
                using var stream = file.OpenReadStream();
                await _blobs.UploadAsync(stream, blobName, file.ContentType);
                TempData["Message"] = $"Uploaded {blobName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Drone image upload failed");
                TempData["Error"] = "Upload failed. Please try again.";
            }

            return RedirectToAction(nameof(Upload));
        }

        [HttpGet]
        public async Task<IActionResult> Gallery()
        {
            var items = await _blobs.ListAsync();
            return View(items);
        }
    }
}
