using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;

using ContosoCrafts.WebSite.Models;
using ContosoCrafts.WebSite.Services;

namespace ContosoCrafts.WebSite.Pages.Product
{
    /// <summary>
    /// The Update page model allows editing of existing product records.
    /// </summary>
    public class UpdateModel : PageModel
    {
        // Service used for accessing and saving product data.
        public JsonFileProductService ProductService
        {
            get;
        }

        // The ProductModel bound to the update form.
        [BindProperty]
        public ProductModel Product
        {
            get;
            set;
        }

        // File upload for a new image.
        [BindProperty]
        public IFormFile Upload
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor initializes the service dependency.
        /// </summary>
        /// <param name="productService">Injected product data service.</param>
        public UpdateModel(JsonFileProductService productService)
        {
            // Allow null service for tests that intentionally supply null; runtime
            // behavior will redirect to Index when the service is missing.
            ProductService = productService;
        }

        /// <summary>
        /// Handles the GET request to populate the form with existing product data.
        /// Fast-fail: redirect to Index when the service or product is missing.
        /// </summary>
        /// <param name="id">The product identifier.</param>
        /// <returns>Page if found; redirects to Index if not.</returns>
        public IActionResult OnGet(string id)
        {
            // Fast fail: no service available
            if (ProductService == null)
            {
                return RedirectToPage("./Index");
            }

            // Attempt to load the product by its ID
            Product = ProductService.GetProducts().FirstOrDefault(p => p.Id.Equals(id));

            // Fast fail: product not found
            if (Product == null)
            {
                return RedirectToPage("./Index");
            }

            // Product loaded successfully � render page
            return Page();
        }


        /// <summary>
        /// Handles POST request to update the product data.
        /// Uses fast-fail for validation and update failure.
        /// </summary>
        /// <returns>Redirect to Index page after successful update.</returns>
        public IActionResult OnPost()
        {
            // Validation-first: if model binding or page-level validation failed, show the page so the user can correct it.
            if (ModelState.IsValid == false)
            {
                return Page();
            }

            // If the bound product or its Id is missing, redirect to Index (product cannot be resolved)
            if (Product == null)
            {
                return RedirectToPage("./Index");
            }

            if (string.IsNullOrWhiteSpace(Product.Id))
            {
                return RedirectToPage("./Index");
            }

            // existingProduct lookup moved further down (after validation) to preserve validation-first behavior.

            // If a new file was uploaded, process it first: delete previous local image and save the new image
            if (Upload == null)
            {
                // No upload provided — skip upload handling
            }

            if (Upload != null)
            {
                if (Upload.Length == 0)
                {
                    // Empty upload — skip
                }

                if (Upload.Length > 0)
                {
                    if (TryHandleUpload(out string uploadError) == false)
                    {
                        ModelState.AddModelError(string.Empty, uploadError);

                        return Page();
                    }
                }
            }

            // Clean graduate and undergraduate degree arrays: remove empty/whitespace entries
            Product.GraduateDegree = (Product.GraduateDegree ?? Array.Empty<string>())
                .Select(s => s?.Trim())
                .Where(s => string.IsNullOrWhiteSpace(s) == false)
                .ToArray();

            Product.UnderGraduateDegree = (Product.UnderGraduateDegree ?? Array.Empty<string>())
                .Select(s => s?.Trim())
                .Where(s => string.IsNullOrWhiteSpace(s) == false)
                .ToArray();

            // Clean campuses list
            Product.Campuses = (Product.Campuses ?? new System.Collections.Generic.List<string>())
                .Select(s => s?.Trim())
                .Where(s => string.IsNullOrWhiteSpace(s) == false)
                .ToList();

            // Note: we clean degree and campus lists above. Do not require them on Update to avoid
            // breaking existing validation expectations (Create enforces them; Update allows empty lists).

            // Extra validation: Ensure critical fields are non-empty
            if (string.IsNullOrWhiteSpace(Product.Title))
            {
                ModelState.AddModelError(string.Empty, "All required fields must be filled before updating.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Product.Description))
            {
                ModelState.AddModelError(string.Empty, "All required fields must be filled before updating.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Product.Url))
            {
                ModelState.AddModelError(string.Empty, "All required fields must be filled before updating.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Product.Image))
            {
                ModelState.AddModelError(string.Empty, "All required fields must be filled before updating.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Product.Location))
            {
                ModelState.AddModelError(string.Empty, "All required fields must be filled before updating.");
                return Page();
            }

            // Enforce max lengths per new requirements
            if (string.IsNullOrWhiteSpace(Product.Title) == false)
            {
                if (Product.Title.Length > 55)
                {
                    ModelState.AddModelError("Product.Title", "Title cannot exceed 55 characters.");

                    return Page();
                }
            }

            if (string.IsNullOrWhiteSpace(Product.Location) == false)
            {
                if (Product.Location.Length > 55)
                {
                    ModelState.AddModelError("Product.Location", "Location cannot exceed 55 characters.");

                    return Page();
                }
            }

            if (string.IsNullOrWhiteSpace(Product.Description) == false)
            {
                if (Product.Description.Length > 500)
                {
                    ModelState.AddModelError("Product.Description", "Description cannot exceed 500 characters.");

                    return Page();
                }
            }

            // Validate each campus name length (max 55 characters)
            if (Product.Campuses != null)
            {
                if (!ValidateCampuses())
                {
                    return Page();
                }
            }
            // After validating input, ensure the product still exists in storage before attempting update.

            var ExistingProduct = ProductService.GetProducts()
                .FirstOrDefault(p => string.Equals(p.Id?.Trim(), Product.Id?.Trim(), System.StringComparison.OrdinalIgnoreCase));

            // If the product was deleted by another process, redirect to Index with a friendly message.
            if (ExistingProduct == null)
            {
                if (TempData != null)
                {
                    TempData["ErrorMessage"] = "Could not update. The product was already deleted.";
                }

                return RedirectToPage("./Index");
            }

            var Result = ProductService.UpdateData(Product);

            // Fast fail: update failed (e.g., product ID not found)
            if (Result == false)
            {
                ModelState.AddModelError(string.Empty, "Update failed. Product not found.");

                return Page();
            }

            // Success redirect to product listing
            return RedirectToPage("./Index");
        }

        // Extracted campus validation so tests can exercise the branch directly.
        private bool ValidateCampuses()
        {
            for (int i = 0; i < Product.Campuses.Count; i++)
            {
                var campus = Product.Campuses[i];
                if (string.IsNullOrWhiteSpace(campus))
                {
                    ModelState.AddModelError($"Product.Campuses[{i}]", $"Campus #{i + 1} cannot be empty.");
                    return false;
                }

                if (campus.Length > 55)
                {
                    ModelState.AddModelError($"Product.Campuses[{i}]", $"Campus #{i + 1} cannot exceed 55 characters.");
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Helper: process the uploaded image file (if any).
        /// Returns true when processing succeeded or no upload was required.
        /// On failure returns false and an error message in <paramref name="error"/>.
        /// </summary>
        /// <param name="error">Output error message when the method returns false.</param>
        /// <returns>True on success; false on failure.</returns>
        private bool TryHandleUpload(out string error)
        {
            error = null;

            if (Upload == null)
            {
                return true;
            }

            if (Upload.Length == 0)
            {
                return true;
            }

            // Delete previous image if it was stored locally under /images/
            var Existing = Product?.Image;

            if (string.IsNullOrWhiteSpace(Existing) == false)
            {
                var Normalized = Existing.Replace("\\", "/");

                if (Normalized.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
                {
                    var RelativePath = Normalized.TrimStart('/');

                    var PhysicalPath = Path.Combine(
                        ProductService.WebHostEnvironment.WebRootPath,
                        RelativePath.Replace('/', Path.DirectorySeparatorChar)
                    );

                    if (System.IO.File.Exists(PhysicalPath))
                    {
                        System.IO.File.Delete(PhysicalPath);
                    }
                }
            }

            // Build filename from the product title (first letter of each word) - Unicode-aware
            var Initials = string.Empty;

            if (Product != null)
            {
                if (string.IsNullOrWhiteSpace(Product.Title) == false)
                {
                    var Matches = System.Text.RegularExpressions.Regex.Matches(Product.Title, "\\p{L}+");

                    var Chars = Matches.Cast<System.Text.RegularExpressions.Match>()
                        .Where(m => m.Length > 0)
                        .Select(m => char.ToUpperInvariant(m.Value[0]));

                    Initials = string.Concat(Chars);
                }
            }

            var Ext = Path.GetExtension(Upload.FileName)?.ToLowerInvariant();

            // Normalize extension to .png or .jpg; default to .png for safety
            if (string.IsNullOrWhiteSpace(Ext))
            {
                Ext = ".png";
            }

            if (string.IsNullOrWhiteSpace(Ext) == false)
            {
                if (Ext != ".png")
                {
                    if (Ext != ".jpg")
                    {
                        if (Ext != ".jpeg")
                        {
                            Ext = ".png";
                        }
                    }
                }
            }

            // normalize .jpeg to .jpg for consistency
            if (Ext == ".jpeg")
            {
                Ext = ".jpg";
            }

            // Sanitize initials to alphanumeric only and limit length, fallback to GUID if empty
            if (string.IsNullOrWhiteSpace(Initials) == false)
            {
                Initials = System.Text.RegularExpressions.Regex.Replace(Initials, "[^A-Z0-9]", "");

                if (Initials.Length > 20)
                {
                    Initials = Initials.Substring(0, 20);
                }
            }

            if (string.IsNullOrWhiteSpace(Initials))
            {
                Initials = Guid.NewGuid().ToString("n").Substring(0, 8);
            }

            var TimeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            var FileName = $"{Initials}_{TimeStamp}{Ext}";

            var ImagesFolder = Path.Combine(ProductService.WebHostEnvironment.WebRootPath, "images");

            if (Directory.Exists(ImagesFolder) == false)
            {
                Directory.CreateDirectory(ImagesFolder);
            }

            var SavePath = Path.Combine(ImagesFolder, FileName);

            // Save the uploaded file to disk
            try
            {
                using (var FileStreamHandle = new FileStream(SavePath, FileMode.Create))
                {
                    Upload.CopyTo(FileStreamHandle);
                }
            }
            catch (System.UnauthorizedAccessException)
            {
                error = "Could not save upload: access denied.";

                return false;
            }
            catch (System.IO.IOException ex)
            {
                // Surface a friendly, testable message and return false so the caller
                // can add a model error and show the page as expected by tests.
                error = "Could not save upload: " + ex.Message;

                return false;
            }

            // Set the product image to the local path used by the site
            Product.Image = "/images/" + FileName;

            return true;
        }
    }
}