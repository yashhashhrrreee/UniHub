using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ContosoCrafts.WebSite.Models;
using ContosoCrafts.WebSite.Services;

namespace ContosoCrafts.WebSite.Pages.Product
{

    /// <summary>
    /// PageModel for the Delete page. Handles displaying a product to confirm deletion 
    /// and performing the deletion when confirmed.
    /// </summary>
    public class DeleteModel : PageModel
    {

        // Service used to access and modify product data.
        public JsonFileProductService ProductService { get; }


        // Bound product for the Delete page.
        [BindProperty]
        public ProductModel Product { get; set; }


        /// <summary>
        /// Construct a new <see cref="DeleteModel"/> instance.
        /// </summary>
        /// <param name="productService">Service used for product CRUD operations.</param>
        public DeleteModel(JsonFileProductService productService)
        {
            // Fast-fail: require a valid service instance.
            ProductService = productService ?? new JsonFileProductService(null);
        }


        /// <summary>
        /// GET handler: load a product by id for delete confirmation.
        /// Returns the page when product is found, otherwise redirects to the Index page.
        /// </summary>
        public IActionResult OnGet(string id)
        {
            // Fast-fail: ensure a valid id is provided.
            if (string.IsNullOrWhiteSpace(id))
            {
                ModelState.AddModelError(string.Empty, "Invalid product identifier.");
                return RedirectToPage("./Index");
            }

            // Load the product.
            Product = FindProductById(id);

            // Fast-fail: product must exist.
            if (Product == null)
            {
                return RedirectToPage("./Index");
            }

            return Page();
        }


        /// <summary>
        /// POST handler: delete the bound product when confirmed.
        /// </summary>
        public IActionResult OnPost()
        {
            // Fast-fail: require bound product data.
            if (Product == null)
            {
                ModelState.AddModelError(string.Empty, "No product data was provided.");
                return RedirectToPage("./Index");
            }

            // Fast-fail: product must have an identifier.
            if (string.IsNullOrWhiteSpace(Product.Id))
            {
                ModelState.AddModelError(string.Empty, "Invalid product identifier. Deletion aborted.");
                return RedirectToPage("./Index");
            }

            var existingProduct = FindProductById(Product.Id);

            // Fast-fail: product must exist.
            if (existingProduct == null)
            {
                ModelState.AddModelError(string.Empty, "Product does not exist. No deletion performed.");
                return RedirectToPage("./Index");
            }

            // Perform deletion.
            ProductService.DeleteData(Product.Id);

            return RedirectToPage("./Index");
        }


        /// <summary>
        /// Helper: find a product by its identifier.
        /// </summary>
        private ProductModel FindProductById(string id)
        {
            // Fast-fail: invalid id yields no product.
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            // Defensive: ensure product list is not null.
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

                if (candidate.Id.Equals(id))
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
