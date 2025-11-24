using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ContosoCrafts.WebSite.Controllers;
using ContosoCrafts.WebSite.Models;
using ContosoCrafts.WebSite.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace UnitTests.Controllers
{
    /// <summary>
    /// Unit tests for ProductsController functionality.
    /// </summary>
    [TestFixture]
    public class ProductsControllerTests
    {
        /// <summary>
        /// Temporary web root path for testing.
        /// </summary>
        private string webRoot;

        /// <summary>
        /// Path to the test data file.
        /// </summary>
        private string dataFile;

        #region Helper Methods

        /// <summary>
        /// Creates a temporary web root folder for test data and returns its path.
        /// </summary>
        private string CreateTempWebRoot(out string dataFile)
        {
            var root = Path.Combine(Path.GetTempPath(), "contoso_tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(root);

            var dataDir = Path.Combine(root, "data");
            Directory.CreateDirectory(dataDir);

            dataFile = Path.Combine(dataDir, "products.json");

            return root;
        }

        #endregion

        #region Get Tests

        /// <summary>
        /// Tests that seeded products are returned correctly with expected titles.
        /// </summary>
        [Test]
        public void Get_ProductsSeeded_ReturnsListWithSeededProductTitle()
        {
            // Arrange
            var Data = new[]
            {
                new ProductModel
                {
                    Id = "p1",
                    Title = "Test Product",
                    Description = "Desc"
                }
            };

            webRoot = CreateTempWebRoot(out dataFile);
            File.WriteAllText(dataFile, JsonSerializer.Serialize(Data));

            var EnvironmentMock = new Mock<IWebHostEnvironment>();
            EnvironmentMock.SetupGet(e => e.WebRootPath).Returns(webRoot);

            var Service = new JsonFileProductService(EnvironmentMock.Object);
            var Controller = new ProductsController(Service);

            // Act
            var Result = Controller.Get();

            // Reset
            // (Cleanup handled in TearDown)

            // Assert
            var ProductList = Result.ToList();
            Assert.AreEqual(1, ProductList.Count);
            Assert.AreEqual("Test Product", ProductList[0].Title);
        }

        #endregion

        #region Patch Tests

        /// <summary>
        /// Tests that adding a rating updates the product and persists the change.
        /// </summary>
        [Test]
        public void Patch_WhenAddingRating_ProductUpdatedAndPersists_ReturnsOk()
        {
            // Arrange
            var Data = new[]
            {
                new ProductModel
                {
                    Id = "p2",
                    Title = "Product Two",
                    Ratings = new int[] { 1 }
                }
            };

            webRoot = CreateTempWebRoot(out dataFile);
            File.WriteAllText(dataFile, JsonSerializer.Serialize(Data));

            var EnvironmentMock = new Mock<IWebHostEnvironment>();
            EnvironmentMock.SetupGet(e => e.WebRootPath).Returns(webRoot);

            var Service = new JsonFileProductService(EnvironmentMock.Object);
            var Controller = new ProductsController(Service);

            var RatingRequest = new ProductsController.RatingRequest
            {
                ProductId = "p2",
                Rating = 5
            };

            // Act
            var Result = Controller.Patch(RatingRequest);

            // Reset
            // (Cleanup handled in TearDown)

            // Assert
            Assert.AreEqual(typeof(OkResult), Result.GetType());

            var JsonContent = File.ReadAllText(dataFile);
            var Items = JsonSerializer.Deserialize<ProductModel[]>(JsonContent);
            Assert.AreEqual(1, Items.Length);
            Assert.AreEqual("p2", Items[0].Id);
            Assert.AreEqual(true, Items[0].Ratings.Contains(5));
        }

        /// <summary>
        /// Tests that null request returns BadRequest.
        /// </summary>
        [Test]
        public void Patch_NullRequest_ReturnsBadRequest()
        {
            // Arrange
            var EnvironmentMock = new Mock<IWebHostEnvironment>();
            EnvironmentMock.SetupGet(e => e.WebRootPath).Returns(".");

            var Service = new JsonFileProductService(EnvironmentMock.Object);
            var Controller = new ProductsController(Service);

            // Act
            var Result = Controller.Patch(null);

            // Assert
            Assert.IsInstanceOf(typeof(Microsoft.AspNetCore.Mvc.BadRequestResult), Result);
        }

        /// <summary>
        /// Tests that empty product ID returns BadRequest with message.
        /// </summary>

        [Test]
        public void Patch_EmptyProductId_ReturnsBadRequestWithMessage()
        {
            // Arrange
            var EnvironmentMock = new Mock<IWebHostEnvironment>();
            EnvironmentMock.SetupGet(e => e.WebRootPath).Returns(".");

            var Service = new JsonFileProductService(EnvironmentMock.Object);
            var Controller = new ProductsController(Service);

            var Request = new ProductsController.RatingRequest { ProductId = "", Rating = 1 };

            // Act
            var Result = Controller.Patch(Request);

            // Assert
            Assert.IsInstanceOf(typeof(Microsoft.AspNetCore.Mvc.BadRequestObjectResult), Result);
        }

        /// <summary>
        /// Tests that whitespace product ID returns BadRequest.
        /// </summary>
        [Test]
        public void Patch_WhitespaceProductId_ReturnsBadRequest()
        {
            // Arrange
            var EnvironmentMock = new Mock<IWebHostEnvironment>();
            EnvironmentMock.SetupGet(e => e.WebRootPath).Returns(".");

            var Service = new JsonFileProductService(EnvironmentMock.Object);
            var Controller = new ProductsController(Service);

            var Request = new ProductsController.RatingRequest { ProductId = "   ", Rating = 2 };

            // Act
            var Result = Controller.Patch(Request);

            // Assert
            Assert.IsInstanceOf(typeof(Microsoft.AspNetCore.Mvc.BadRequestObjectResult), Result);
        }

        /// <summary>
        /// Tests that non-existent product still returns Ok (service handles gracefully).
        /// </summary>
        [Test]
        public void Patch_NonExistentProduct_AddRatingReturnsFalse_ControllerReturnsOk()
        {
            // Arrange
            webRoot = CreateTempWebRoot(out dataFile);
            File.WriteAllText(dataFile, JsonSerializer.Serialize(new ProductModel[0]));

            var EnvironmentMock = new Mock<IWebHostEnvironment>();
            EnvironmentMock.SetupGet(e => e.WebRootPath).Returns(webRoot);

            var Service = new JsonFileProductService(EnvironmentMock.Object);
            var Controller = new ProductsController(Service);

            var Request = new ProductsController.RatingRequest { ProductId = "no-such-id", Rating = 3 };

            // Act
            var Result = Controller.Patch(Request);

            // Assert
            Assert.IsInstanceOf(typeof(OkResult), Result);
        }

        #endregion

        #region Constructor Tests

        /// <summary>
        /// Tests that constructor handles null service gracefully by using default service.
        /// </summary>
        [Test]
        public void Constructor_NullService_UsesDefaultService()
        {
            // Act
            var controller = new ProductsController(null);

            // Assert
            Assert.IsNotNull(controller.ProductService, "ProductService should not be null when null is passed to constructor.");
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleans up temporary test directories after each test.
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
            if (string.IsNullOrEmpty(webRoot) == false)
            {
                if (Directory.Exists(webRoot) == true)
                {
                    Directory.Delete(webRoot, true);
                }
            }
        }

        #endregion
    }
}
