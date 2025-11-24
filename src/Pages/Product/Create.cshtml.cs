using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ContosoCrafts.WebSite.Models;
using Microsoft.AspNetCore.Hosting;
using ContosoCrafts.WebSite.Services;

namespace ContosoCrafts.WebSite.Pages.Product
{
    /// <summary>
    /// Page model for creating new product records.
    /// </summary>
    public class CreateModel : PageModel
    {
        // Product data service used to persist new product records
        private readonly JsonFileProductService _productService;

        // Web host environment used to resolve web root paths
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Logger instance for this page model
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(JsonFileProductService productService, IWebHostEnvironment webHostEnvironment, ILogger<CreateModel> logger)
        {
            // Use null coalescing pattern with safe defaults
            _productService = productService ?? new JsonFileProductService(null);
            _webHostEnvironment = webHostEnvironment; // Keep null if null - the code should handle it gracefully
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<CreateModel>.Instance;
        }

        // Bound Product model for the form submission
        [BindProperty]
        public ProductModel Product
        {
            get;
            set;
        } = new ProductModel();

        /// <summary>
        /// Handles GET requests for the page.
        /// </summary>
        public void OnGet()
        {
            // Ensure Product has defaults
            Product ??= new ProductModel();
        }

        /// <summary>
        /// Handles POST requests when the form is submitted.
        /// </summary>
        public IActionResult OnPost()
        {
            // ✔ fast-fail added
            if (Product == null)
            {
                Product = new ProductModel();
            }

            CleanDegreeArrays();

            CleanCampuses();

            ValidateCampuses();

            ValidateDegreeArray("Product.GraduateDegree", Product.GraduateDegree, "Graduate degree");

            ValidateDegreeArray("Product.UnderGraduateDegree", Product.UnderGraduateDegree, "Undergraduate degree");

            if (ModelState.IsValid == false)
            {
                return Page();
            }

            Product.Id = GenerateIdFromTitle(Product.Title);
            _productService.CreateData(Product);

            return RedirectToPage("./Index");
        }

        /// <summary>
        /// Handles AJAX upload of image file.
        /// </summary>
        public async Task<JsonResult> OnPostUploadImageAsync(IFormFile image, string title)
        {
            // Validate file existence (fast-fail with independent checks)
            if (image == null)
            {
                return new JsonResult(new { success = false, error = "No file provided." });
            }

            if (image.Length == 0)
            {
                return new JsonResult(new { success = false, error = "No file provided." });
            }

            const long maxBytes = 2 * 1024 * 1024; // 2 MB

            if (image.Length > maxBytes)
            {
                return new JsonResult(new { success = false, error = "File is too large. Maximum allowed size is 2 MB." });
            }

            byte[] fileBytes;

            using (var ms = new MemoryStream())
            {
                await image.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            // Basic magic-bytes sniffing
            var pngDetected = IsPng(fileBytes);
            var jpegDetected = IsJpeg(fileBytes);

            if (pngDetected == false)
            {
                if (jpegDetected == false)
                {
                    return new JsonResult(new { success = false, error = "Uploaded file is not a supported image type (png/jpg/jpeg)." });
                }
            }

            var extension = ".jpg";

            if (pngDetected)
            {
                extension = ".png";
            }

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

            Directory.CreateDirectory(uploadsFolder);

            var initials = BuildInitialsFromTitle(title);
            var timeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var fileName = $"{initials}_{timeStamp}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

            var webPath = $"/images/{fileName}";

            return new JsonResult(new { success = true, imagePath = webPath });
        }

        private static bool IsPng(byte[] fileBytes)
        {
            if (fileBytes == null)
            {
                return false;
            }

            if (fileBytes.Length < 8)
            {
                return false;
            }

            if (fileBytes[0] != 0x89)
            {
                return false;
            }

            if (fileBytes[1] != 0x50)
            {
                return false;
            }

            if (fileBytes[2] != 0x4E)
            {
                return false;
            }

            if (fileBytes[3] != 0x47)
            {
                return false;
            }

            if (fileBytes[4] != 0x0D)
            {
                return false;
            }

            if (fileBytes[5] != 0x0A)
            {
                return false;
            }

            if (fileBytes[6] != 0x1A)
            {
                return false;
            }

            if (fileBytes[7] != 0x0A)
            {
                return false;
            }

            return true;
        }

        private static bool IsJpeg(byte[] fileBytes)
        {
            if (fileBytes == null)
            {
                return false;
            }

            if (fileBytes.Length < 2)
            {
                return false;
            }

            if (fileBytes[0] != 0xFF)
            {
                return false;
            }

            if (fileBytes[1] != 0xD8)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Delete an uploaded image.
        /// </summary>
        public JsonResult OnPostDeleteImageAsync(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return new JsonResult(new { success = false, error = "No image path provided." });
            }

            var normalized = imagePath.Replace("\\", "/");

            if (normalized.StartsWith("/images/", StringComparison.OrdinalIgnoreCase) == false)
            {
                return new JsonResult(new { success = false, error = "Invalid image path." });
            }

            var relativePath = normalized.TrimStart('/');
            var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }

            return new JsonResult(new { success = true });
        }

        /// <summary>
        /// Generate ID from title.
        /// </summary>
        private string GenerateIdFromTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Guid.NewGuid().ToString("N").Substring(0, 8);
            }

            var matches = Regex.Matches(title, "\\p{L}+");

            if (matches.Count == 0)
            {
                return Guid.NewGuid().ToString("N").Substring(0, 8);
            }

            var chars = matches
                .Cast<Match>()
                .Where(m => m.Length > 0)
                .Select(m => char.ToLowerInvariant(m.Value[0]));

            var id = string.Concat(chars);

            id = Regex.Replace(id, "[^a-z0-9]", "");

            if (string.IsNullOrWhiteSpace(id))
            {
                return Guid.NewGuid().ToString("N").Substring(0, 8);
            }

            return id;
        }

        /// <summary>
        /// Parse a delimited degree list.
        /// </summary>
        private string[] ParseDegrees(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return Array.Empty<string>();
            }

            return input
                .Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => string.IsNullOrWhiteSpace(s) == false)
                .ToArray();
        }

        /// <summary>
        /// Clean degree arrays.
        /// </summary>
        private void CleanDegreeArrays()
        {
            Product.GraduateDegree = (Product.GraduateDegree ?? Array.Empty<string>())
                .Select(s => s?.Trim())
                .Where(s => string.IsNullOrWhiteSpace(s) == false)
                .ToArray();

            Product.UnderGraduateDegree = (Product.UnderGraduateDegree ?? Array.Empty<string>())
                .Select(s => s?.Trim())
                .Where(s => string.IsNullOrWhiteSpace(s) == false)
                .ToArray();
        }

        /// <summary>
        /// Clean campuses list.
        /// </summary>
        private void CleanCampuses()
        {
            var cleanedCampuses = (Product.Campuses ?? new System.Collections.Generic.List<string>())
                .Select(s => s?.Trim())
                .Where(s => string.IsNullOrWhiteSpace(s) == false)
                .ToList();

            Product.Campuses = cleanedCampuses;
        }

        /// <summary>
        /// Validate campus entries.
        /// </summary>
        private void ValidateCampuses()
        {
            if (Product.Campuses == null)
            {
                ModelState.AddModelError("Product.Campuses", "At least one campus is required.");
                return;
            }

            if (Product.Campuses.Count == 0)
            {
                ModelState.AddModelError("Product.Campuses", "At least one campus is required.");
                return;
            }

            for (var index = 0; index < Product.Campuses.Count; index++)
            {
                var campus = Product.Campuses[index];

                if (string.IsNullOrWhiteSpace(campus))
                {
                    ModelState.AddModelError($"Product.Campuses[{index}]", $"Campus #{index + 1} cannot be empty.");
                    continue;
                }

                if (campus.Length > 55)
                {
                    ModelState.AddModelError($"Product.Campuses[{index}]", $"Campus #{index + 1} cannot exceed 55 characters.");
                }
            }
        }

        /// <summary>
        /// Validate a degree array.
        /// </summary>
        private void ValidateDegreeArray(string propertyName, string[] degrees, string displayName)
        {
            if (degrees == null)
            {
                ModelState.AddModelError(propertyName, $"At least one {displayName.ToLowerInvariant()} is required.");
                return;
            }

            if (degrees.Length == 0)
            {
                ModelState.AddModelError(propertyName, $"At least one {displayName.ToLowerInvariant()} is required.");
                return;
            }

            for (var index = 0; index < degrees.Length; index++)
            {
                var degree = degrees[index];

                if (string.IsNullOrWhiteSpace(degree))
                {
                    ModelState.AddModelError($"{propertyName}[{index}]", $"{displayName} #{index + 1} cannot be empty.");
                    continue;
                }

                if (degree.Length > 100)
                {
                    ModelState.AddModelError($"{propertyName}[{index}]", $"{displayName} #{index + 1} cannot exceed 100 characters.");
                }
            }
        }

        /// <summary>
        /// Build title initials.
        /// </summary>
        private string BuildInitialsFromTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Guid.NewGuid().ToString("N").Substring(0, 8);
            }

            var matches = Regex.Matches(title, "\\p{L}+");

            var chars = matches
                .Cast<Match>()
                .Where(m => m.Length > 0)
                .Select(m => char.ToUpperInvariant(m.Value[0]));

            var initials = string.Concat(chars);

            if (string.IsNullOrWhiteSpace(initials) == false)
            {
                initials = Regex.Replace(initials, "[^A-Z0-9]", "");

                if (initials.Length > 20)
                {
                    initials = initials.Substring(0, 20);
                }
            }

            if (string.IsNullOrWhiteSpace(initials))
            {
                initials = Guid.NewGuid().ToString("N").Substring(0, 8);
            }

            return initials;
        }
    }
}
