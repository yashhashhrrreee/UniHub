using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using ContosoCrafts.WebSite.Models;
using ContosoCrafts.WebSite.Services;

namespace ContosoCrafts.WebSite.Pages.Product
{
    /// <summary>
    /// Page model for displaying a single product (Read page).
    /// </summary>
    public class ReadModel : PageModel
    {
        // Product data service.
        public JsonFileProductService ProductService { get; }

        // Product to display on the page.
        public ProductModel Product { get; private set; }

        /// <summary>
        /// Constructor initializes the data service.
        /// </summary>
        /// <param name="productService">Injected product service.</param>
        public ReadModel(JsonFileProductService productService)
        {
            // Allow null for tests; runtime handles fast-fail.
            ProductService = productService;
        }

        /// <summary>
        /// Handles GET requests and loads the product by id.
        /// Fast-fail: if service or product not found, redirect to Index.
        /// </summary>
        /// <param name="id">Product identifier.</param>
        public IActionResult OnGet(string id)
        {
            // Fast-fail: ensure service is available.
            if (ProductService == null)
            {
                return RedirectToPage("./Index");
            }

            // Fast-fail: ensure id is provided.
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToPage("./Index");
            }

            Product = FindProductById(id);

            // Fast-fail: product must exist.
            if (Product == null)
            {
                return RedirectToPage("./Index");
            }

            return Page();
        }


        /// <summary>
        /// Finds and returns a product that matches the specified product ID.
        /// </summary>
        /// <param name="id">Product identifier.</param>
        /// <returns>
        /// Matching product or null if not found.
        /// </returns>
        private ProductModel FindProductById(string id)
        {
            // Fast-fail: invalid id.
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            // Defensive: if service or products list is null, use empty array.
            ProductModel[] products;
            if (ProductService is null)
                products = System.Array.Empty<ProductModel>();
            else
                products = ProductService.GetProducts().ToArray();

            foreach (var candidate in products)
            {
                if (candidate == null)
                {
                    continue;
                }

                if (candidate.Id == null)
                {
                    continue;
                }

                // Match found.
                if (candidate.Id.Equals(id))
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
