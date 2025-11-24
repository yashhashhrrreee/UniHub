using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using ContosoCrafts.WebSite.Models;
using ContosoCrafts.WebSite.Services;

namespace ContosoCrafts.WebSite.Pages
{
    /// <summary>
    /// Page model for the Index page.
    /// Responsible for retrieving and displaying the list of available products
    /// from the JSON data source using the <see cref="JsonFileProductService"/>.
    /// </summary>
    public class IndexModel : PageModel
    {
        // Logger used to record page lifecycle events.
        private readonly ILogger<IndexModel> Logger;


        /// <summary>
        /// Initializes a new instance of the <see cref="IndexModel"/> class.
        /// </summary>
        /// <param name="logger">Injected logger used for tracking page lifecycle events.</param>
        /// <param name="productService">Injected product service for data retrieval.</param>
        public IndexModel(ILogger<IndexModel> logger,
            JsonFileProductService productService)
        {
            // Fast-fail: require non-null dependencies.
            Logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<IndexModel>.Instance;

            ProductService = productService ?? new JsonFileProductService(null);
        }

        // The data service that provides access to product data.
        public JsonFileProductService ProductService { get; }

        // Collection of product records loaded from the data source.
        public IEnumerable<ProductModel> Products { get; private set; }

        /// <summary>
        /// Handles GET requests for the Index page.
        /// Loads the complete list of products from the JSON file via the data service.
        /// </summary>
        public void OnGet()
        {
            // Defensive: if ProductService is unexpectedly null, return an empty collection.
            if (ProductService == null)
            {
                Products = new ProductModel[0];

                return;
            }

            // Retrieve all products and assign to Products collection.
            Products = ProductService.GetProducts();
        }
    }
}