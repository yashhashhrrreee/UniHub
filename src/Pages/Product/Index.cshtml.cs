using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using ContosoCrafts.WebSite.Models;
using ContosoCrafts.WebSite.Services;

namespace ContosoCrafts.WebSite.Pages.Product
{
    /// <summary>
    /// Index Page returns all product data to display.
    /// </summary>
    public class IndexModel : PageModel
    {

        /// <summary>
        /// Data Service used to access products.
        /// </summary>
        public JsonFileProductService ProductService
        {
            get;
        }

        /// <summary>
        /// Collection of product data shown on the page.
        /// </summary>
        public IEnumerable<ProductModel> Products
        {
            get;
            private set;
        }

        /// <summary>
        /// Bound property for search term (from query string).
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string SearchTerm
        {
            get;
            set;
        }

        /// <summary>
        /// Bound property for type filter (from query string).
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string TypeFilter
        {
            get;
            set;
        }
        
        /// <summary>
        /// Constructor initializes the data service.
        /// </summary>
        public IndexModel(JsonFileProductService productService)
        {
            // Allow null for unit tests
            ProductService = productService;
        }
        
        /// <summary>
        /// Handles GET requests and loads all products.
        /// Uses fast-fail: if the service is not available, returns an empty collection.
        /// </summary>
        public void OnGet()
        {
            // Fast-fail: if ProductService is missing.
            if (ProductService == null)
            {
                Products = new ProductModel[0];
                return;
            }

            // Load products from service.
            var allProducts = ProductService.GetProducts();

            // Start query with all products.
            var query = allProducts.AsEnumerable();
            
            if (string.IsNullOrWhiteSpace(SearchTerm) == false)   
            {
                var byTitle = query.Where(product => TitleContains(product, SearchTerm));
                var byDescription = query.Where(product => DescriptionContains(product, SearchTerm));

                query = byTitle.Concat(byDescription).Distinct();
            }
            
            if (string.IsNullOrWhiteSpace(TypeFilter) == false)   
            {
                var parsedOk = System.Enum.TryParse<ProductTypeEnum>(TypeFilter, true, out var parsed);

                if (parsedOk)
                {
                    query = query.Where(product => product.TypeOfUniversity == parsed);
                }
            }

            Products = query;
        }


        /// <summary>
        /// Checks if the product's title contains the search term.
        /// </summary>
        private bool TitleContains(ProductModel product, string term)
        {
            if (product == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(term))
            {
                return false;
            }

            if (product.Title == null)
            {
                return false;
            }

            return product.Title.Contains(term, System.StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Checks if the product's description contains the search term.
        /// </summary>
        private bool DescriptionContains(ProductModel product, string term)
        {
            if (product == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(term))
            {
                return false;
            }

            if (product.Description == null)
            {
                return false;
            }

            return product.Description.Contains(term, System.StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Returns the CSS flag class based on the university type.
        /// </summary>
        public string GetFlagClass(ProductTypeEnum type)
        {
            return type switch
            {
                ProductTypeEnum.Public => "flag-public",
                ProductTypeEnum.Private => "flag-private",
                ProductTypeEnum.Online => "flag-online",
                ProductTypeEnum.Community => "flag-community",
                ProductTypeEnum.Other => "flag-other",
                _ => "flag-undefined"
            };
        }
    }
}