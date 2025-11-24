using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ContosoCrafts.WebSite.Models;
using ContosoCrafts.WebSite.Services;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace ContosoCrafts.WebSite.Controllers
{
    /// <summary>
    /// Controller responsible for handling API requests related to Products.
    /// Provides endpoints for retrieving and updating product data.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [ExcludeFromCodeCoverage]
    public class ProductsController : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductsController"/> class.
        /// </summary>
        /// <param name="productService">The injected product service for data operations.</param>
        public ProductsController(JsonFileProductService productService)
        {
            // If no service is provided, create a default one
            if (productService == null)
            {
                var mockEnvironment = new MockWebHostEnvironment();
                ProductService = new JsonFileProductService(mockEnvironment);
            }
            else
            {
                ProductService = productService;
            }
        }

        // Gets the injected product service instance.
        public JsonFileProductService ProductService
        {
            get;
        }

        /// <summary>
        /// GET endpoint to retrieve all products.
        /// </summary>
        /// <returns>A collection of <see cref="ProductModel"/> objects.</returns>
        [HttpGet]
        public IEnumerable<ProductModel> Get()
        {
            return ProductService.GetProducts();
        }

        /// <summary>
        /// PATCH endpoint to add a rating to a product.
        /// </summary>
        /// <param name="request">Request object containing the Product ID and Rating.</param>
        /// <returns>Returns HTTP 200 OK if successful.</returns>
        [HttpPatch]
        public ActionResult Patch([FromBody] RatingRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(request.ProductId))
            {
                return BadRequest("ProductId is required.");
            }

            ProductService.AddRating(request.ProductId, request.Rating);

            return Ok();
        }

        /// <summary>
        /// Internal class representing a rating update request.
        /// </summary>
        public class RatingRequest
        {
            // Unique Product ID for which the rating applies.
            // Gets or sets the unique Product ID.
            public string ProductId
            {
                get;
                set;
            }

            // Gets or sets the numeric rating value.
            public int Rating
            {
                get;
                set;
            }
        }
    }

    /// <summary>
    /// Mock implementation of IWebHostEnvironment for testing purposes.
    /// </summary>
    public class MockWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider WebRootFileProvider { get; set; }
        public string ApplicationName { get; set; } = "ContosoCrafts.WebSite";
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public string EnvironmentName { get; set; } = "Development";
    }
}