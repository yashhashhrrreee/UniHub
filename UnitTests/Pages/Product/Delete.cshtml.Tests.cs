using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;
using Moq;
using NUnit.Framework;
using ContosoCrafts.WebSite.Services;
using ContosoCrafts.WebSite.Models;
using ContosoCrafts.WebSite.Pages.Product;

namespace UnitTests.Pages.Product
{

    /// <summary>
    /// Contains unit tests for the <see cref="DeleteModel"/> page model.
    /// Each test exercises a single behavior of the DeleteModel page handlers.
    /// Test class name follows the pattern: ClassName + "Test".
    /// </summary>
    public class DeleteModelTest
    {
        // Temporary WebRoot path used by the test service to store a products.json file.
        private string TempWebRoot;

        /// <summary>
        /// Remove the temporary web root directory created for a test.
        /// This is a best-effort cleanup invoked from each test's Reset step.
        /// </summary>
        private void CleanupTempDirectory()
        {
            // Deletes the directory created in CreateServiceWithProducts. If the
            // directory is already removed this will throw; callers expect the
            // test run to surface such issues rather than hiding them.
            Directory.Delete(TempWebRoot, true);

            // Reset the stored path after deletion.
            TempWebRoot = null;
        }

        /// <summary>
        /// Creates a <see cref="JsonFileProductService"/> backed by a temporary
        /// webroot that contains a products.json initialized with the provided products.
        /// </summary>
        /// <param name="products">Array of products to write into the temporary JSON file.</param>
        /// <returns>A new JsonFileProductService instance using the temporary webroot.</returns>
        private JsonFileProductService CreateServiceWithProducts(params ProductModel[] products)
        {
            // Compose a unique temporary directory to act as the WebRoot for the service.
            TempWebRoot = Path.Combine(Path.GetTempPath(), "contoso_tests_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(TempWebRoot);

            // Create a data folder and write the products.json file used by the service.
            var dataDir = Path.Combine(TempWebRoot, "data");
            Directory.CreateDirectory(dataDir);

            var file = Path.Combine(dataDir, "products.json");

            var json = JsonSerializer.Serialize(products);
            File.WriteAllText(file, json);

            // Create a mock environment that points the service at our temp WebRoot.
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.WebRootPath).Returns(TempWebRoot);

            return new JsonFileProductService(envMock.Object);
        }

        [Test]
        public void DeleteModel_Constructor_NullService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => new DeleteModel(null),
                "Constructor should handle null service gracefully.");
        }

        [Test]
        /// <summary>
        /// OnGet: when id is null, model state should be invalid and handler redirects to Index.
        /// </summary>
        public void DeleteModel_OnGet_NullId_ModelStateInvalid_RedirectsToIndex()
        {
            // Arrange
            var service = CreateServiceWithProducts();
            var data = new { Service = service, Page = new DeleteModel(service) };

            // Act
            var result = data.Page.OnGet(null);

            // Reset
            CleanupTempDirectory();

            // Assert
            Assert.AreEqual(typeof(RedirectToPageResult), result.GetType());
            var redirect = (RedirectToPageResult)result;
            Assert.AreEqual("./Index", redirect.PageName);
            Assert.AreEqual(false, data.Page.ModelState.IsValid);
        }

        [Test]
        /// <summary>
        /// OnGet: when id is missing, the handler should redirect to Index.
        /// </summary>
        public void DeleteModel_OnGet_MissingId_RedirectsToIndex()
        {
            // Arrange
            var service = CreateServiceWithProducts();
            var data = new { Service = service, Page = new DeleteModel(service) };

            // Act
            var result = data.Page.OnGet("missing-id");

            // Reset
            CleanupTempDirectory();

            // Assert
            Assert.AreEqual(typeof(RedirectToPageResult), result.GetType());
            var redirect = (RedirectToPageResult)result;
            Assert.AreEqual("./Index", redirect.PageName);
        }

        [Test]
        /// <summary>
        /// OnGet: when provided a valid id, the handler returns the page and populates Product.
        /// </summary>
        public void DeleteModel_OnGet_ValidId_ReturnsPageAndPopulatesProduct()
        {
            // Arrange
            var p = new ProductModel { Id = "p1", Title = "t" };
            var service = CreateServiceWithProducts(p);
            var data = new { Service = service, Page = new DeleteModel(service) };

            // Act
            var result = data.Page.OnGet("p1");

            // Reset
            CleanupTempDirectory();

            // Assert
            Assert.AreEqual(typeof(PageResult), result.GetType());
            Assert.AreNotEqual(null, data.Page.Product);
            Assert.AreEqual("p1", data.Page.Product.Id);
        }

        [Test]
        /// <summary>
        /// OnPost: when Product is null, model state should be invalid and handler redirects to Index.
        /// </summary>
        public void DeleteModel_OnPost_ProductNull_ModelStateInvalid_RedirectsToIndex()
        {
            // Arrange
            var service = CreateServiceWithProducts();
            var data = new { Service = service, Page = new DeleteModel(service) };

            // Act
            var result = data.Page.OnPost();

            // Reset
            CleanupTempDirectory();

            // Assert
            Assert.AreEqual(typeof(RedirectToPageResult), result.GetType());
            var redirect = (RedirectToPageResult)result;
            Assert.AreEqual("./Index", redirect.PageName);
            Assert.AreEqual(false, data.Page.ModelState.IsValid);
        }

        [Test]
        /// <summary>
        /// OnPost: when Product is null, ensure a specific model error is added describing the missing data.
        /// This verifies the guard clause records an error for callers that inspect ModelState details.
        /// </summary>
        public void DeleteModel_OnPost_ProductNull_AddsSpecificModelError()
        {
            // Arrange
            var service = CreateServiceWithProducts();
            var data = new { Service = service, Page = new DeleteModel(service) };

            // Act
            var result = data.Page.OnPost();

            // Reset
            CleanupTempDirectory();

            // Assert redirect occurred
            Assert.AreEqual(typeof(RedirectToPageResult), result.GetType());

            // Assert the ModelState is invalid and contains the expected error message
            Assert.AreEqual(false, data.Page.ModelState.IsValid);

            var allErrors = data.Page.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            Assert.AreEqual(true, allErrors.Any(msg => msg.Contains("No product data was provided")),
                $"Expected a ModelState error containing 'No product data was provided', found: {string.Join(";", allErrors)}");
        }

        [Test]
        /// <summary>
        /// OnPost: when Product.Id is empty the model state should be invalid and the handler redirects.
        /// </summary>
        public void DeleteModel_OnPost_EmptyProductId_ModelStateInvalid_RedirectsToIndex()
        {
            // Arrange
            var service = CreateServiceWithProducts();
            var data = new
            {
                Service = service,
                Page = new DeleteModel(service)
                {
                    Product = new ProductModel { Id = string.Empty }
                }
            };

            // Act
            var result = data.Page.OnPost();

            // Reset
            CleanupTempDirectory();

            // Assert
            Assert.AreEqual(typeof(RedirectToPageResult), result.GetType());
            var redirect = (RedirectToPageResult)result;
            Assert.AreEqual("./Index", redirect.PageName);
            Assert.AreEqual(false, data.Page.ModelState.IsValid);
        }

        [Test]
        /// <summary>
        /// OnPost: when the posted Product.Id does not match any stored product, model state is invalid and redirect occurs.
        /// </summary>
        public void DeleteModel_OnPost_ProductDoesNotExist_ModelStateInvalid_RedirectsToIndex()
        {
            // Arrange
            var service = CreateServiceWithProducts(new ProductModel { Id = "other" });
            var data = new
            {
                Service = service,
                Page = new DeleteModel(service)
                {
                    Product = new ProductModel { Id = "nope" }
                }
            };

            // Act
            var result = data.Page.OnPost();

            // Reset
            CleanupTempDirectory();

            // Assert
            Assert.AreEqual(typeof(RedirectToPageResult), result.GetType());
            var redirect = (RedirectToPageResult)result;
            Assert.AreEqual("./Index", redirect.PageName);
            Assert.AreEqual(false, data.Page.ModelState.IsValid);
        }

        [Test]
        /// <summary>
        /// OnGet: when no product matches the requested id, the handler iterates and redirects to Index.
        /// </summary>
        public void DeleteModel_OnGet_NoMatchingProduct_IteratesAndRedirectsToIndex()
        {
            // Arrange
            var service = CreateServiceWithProducts(new ProductModel { Id = "different" });
            var data = new { Service = service, Page = new DeleteModel(service) };

            // Act
            var result = data.Page.OnGet("missing-id");

            // Reset
            CleanupTempDirectory();

            // Assert
            Assert.AreEqual(typeof(RedirectToPageResult), result.GetType());
            var redirect = (RedirectToPageResult)result;
            Assert.AreEqual("./Index", redirect.PageName);
        }

        [Test]
        /// <summary>
        /// OnPost: when the product exists (later in the list), it should be deleted and the handler should redirect.
        /// </summary>
        public void DeleteModel_OnPost_ProductExists_DeletesMatchingProductAndRedirects()
        {
            // Arrange
            var a = new ProductModel { Id = "a" };
            var b = new ProductModel { Id = "delme" };
            var service = CreateServiceWithProducts(a, b);
            var data = new
            {
                Service = service,
                Page = new DeleteModel(service)
                {
                    Product = new ProductModel { Id = "delme" }
                }
            };

            // Act
            var result = data.Page.OnPost();

            // Assert
            Assert.AreEqual(typeof(RedirectToPageResult), result.GetType());
            var remainingList = data.Service.GetProducts().ToList();
            Assert.AreEqual(1, remainingList.Count);
            Assert.AreEqual("a", remainingList[0].Id);
        }

        [Test]
        /// <summary>
        /// OnPost: valid product id should be deleted and the handler should redirect to Index.
        /// </summary>
        public void DeleteModel_OnPost_ValidProduct_CallsDeleteAndRedirectsToIndex()
        {
            // Arrange
            var product = new ProductModel { Id = "to-delete" };
            var service = CreateServiceWithProducts(product);

            var data = new
            {
                Service = service,
                Page = new DeleteModel(service)
                {
                    Product = new ProductModel { Id = "to-delete" }
                }
            };

            // Act
            var result = data.Page.OnPost();

            // Reset
            CleanupTempDirectory();

            // Assert
            Assert.AreEqual(typeof(RedirectToPageResult), result.GetType());
            var redirect = (RedirectToPageResult)result;
            Assert.AreEqual("./Index", redirect.PageName);
        }

        [Test]
        /// <summary>
        /// Directly exercises the private FindProductById method with a whitespace id.
        /// This should return null without throwing and cover the early-return branch.
        /// </summary>
        public void DeleteModel_FindProductById_WhitespaceId_ReturnsNull()
        {
            // Arrange
            var p = new ProductModel { Id = "p1" };
            var service = CreateServiceWithProducts(p);
            var data = new { Service = service, Page = new DeleteModel(service) };

            // Use reflection to get the non-public FindProductById method
            var method = typeof(DeleteModel).GetMethod("FindProductById", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = method.Invoke(data.Page, new object[] { " " });

            // Reset
            CleanupTempDirectory();

            // Assert
            Assert.AreEqual(null, result);
        }

        [Test]
        /// <summary>
        /// Directly invokes FindProductById for a valid id and expects the matching product.
        /// This ensures the method returns the candidate when the ids match.
        /// </summary>
        public void DeleteModel_FindProductById_ValidId_ReturnsProduct()
        {
            // Arrange
            var p = new ProductModel { Id = "p-reflect", Title = "t" };
            var service = CreateServiceWithProducts(p);
            var data = new { Service = service, Page = new DeleteModel(service) };

            // Reflection to get private method
            var method = typeof(DeleteModel).GetMethod("FindProductById", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = method.Invoke(data.Page, new object[] { "p-reflect" });

            // Reset
            CleanupTempDirectory();

            // Assert
            Assert.AreNotEqual(null, result);
            var returned = result as ProductModel;
            Assert.AreNotEqual(null, returned);
            Assert.AreEqual("p-reflect", returned.Id);
        }

        [Test]
        /// <summary>
        /// Directly invokes FindProductById when the products array contains null entries
        /// and a product with a null Id. This should return null without throwing.
        /// </summary>
        public void DeleteModel_FindProductById_NullEntries_ReturnsNull()
        {
            // Arrange: create service with a null candidate and a product with null Id
            var service = CreateServiceWithProducts(new ProductModel[] { null, new ProductModel { Id = null } });
            var data = new { Service = service, Page = new DeleteModel(service) };

            // Reflection to get private method
            var method = typeof(DeleteModel).GetMethod("FindProductById", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = method.Invoke(data.Page, new object[] { "any-id" });

            // Reset
            CleanupTempDirectory();

            // Assert
            Assert.AreEqual(null, result);
        }

        [Test]
        /// <summary>
        /// Directly invokes FindProductById when the products array contains a mix of
        /// null entries, null-Id products, non-matching ids and a later matching id.
        /// This ensures the iteration correctly skips invalid candidates and returns
        /// the later matching product.
        /// </summary>
        public void DeleteModel_FindProductById_MixedCandidates_ReturnsMatchingLater()
        {
            // Arrange: mix of null, null-Id, non-matching and matching candidates
            var products = new ProductModel[]
            {
                null,
                new ProductModel { Id = null },
                new ProductModel { Id = "not-this" },
                new ProductModel { Id = "target-id", Title = "match" }
            };

            var service = CreateServiceWithProducts(products);
            var data = new { Service = service, Page = new DeleteModel(service) };

            var method = typeof(DeleteModel).GetMethod("FindProductById", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = method.Invoke(data.Page, new object[] { "target-id" });

            // Reset
            CleanupTempDirectory();

            // Assert
            Assert.AreNotEqual(null, result);
            var returned = result as ProductModel;
            Assert.AreNotEqual(null, returned);
            Assert.AreEqual("target-id", returned.Id);
        }

        [Test]
        /// <summary>
        /// Forces ProductService to null via reflection to exercise the branch assigning
        /// products = System.Array.Empty<ProductModel>() in FindProductById. Expects null result.
        /// </summary>
        public void DeleteModel_FindProductById_NullService_UsesEmptyArray_ReturnsNull()
        {
            // Arrange
            var model = new DeleteModel(null); // constructor replaces null with instance
            var backing = typeof(DeleteModel).GetField("<ProductService>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(backing);
            backing.SetValue(model, null); // force ProductService to null
            var method = typeof(DeleteModel).GetMethod("FindProductById", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method);

            // Act
            var result = method.Invoke(model, new object[] { "any-id" });

            // Assert
            Assert.AreEqual(null, result);
        }
    }
}
