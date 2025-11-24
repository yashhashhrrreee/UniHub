
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

using Moq;
using System;
using System.IO;
using System.Reflection;

using NUnit.Framework;

using ContosoCrafts.WebSite.Pages.Product;
using ContosoCrafts.WebSite.Services;

namespace UnitTests.Pages.Product
{

    /// <summary>
    /// Unit tests for the <see cref="ReadModel"/> page model.
    /// Test class name follows the pattern: ClassName + "Test".
    /// </summary>
    public class ReadModelTest
    {
        #region TestSetup
        public static IUrlHelperFactory urlHelperFactory;
        public static DefaultHttpContext httpContextDefault;
        public static IWebHostEnvironment webHostEnvironment;
        public static ModelStateDictionary modelState;
        public static ActionContext actionContext;
        public static EmptyModelMetadataProvider modelMetadataProvider;
        public static ViewDataDictionary viewData;
        public static TempDataDictionary tempData;
        public static PageContext pageContext;

        public static ReadModel pageModel;

        [SetUp]
        public void TestInitialize()
        {
            httpContextDefault = new DefaultHttpContext()
            {
                //RequestServices = serviceProviderMock.Object,
            };

            modelState = new ModelStateDictionary();

            actionContext = new ActionContext(httpContextDefault, httpContextDefault.GetRouteData(), new PageActionDescriptor(), modelState);

            modelMetadataProvider = new EmptyModelMetadataProvider();
            viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
            tempData = new TempDataDictionary(httpContextDefault, Mock.Of<ITempDataProvider>());

            pageContext = new PageContext(actionContext)
            {
                ViewData = viewData,
            };

            var mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            mockWebHostEnvironment.Setup(m => m.EnvironmentName).Returns("Hosting:UnitTestEnvironment");
            mockWebHostEnvironment.Setup(m => m.WebRootPath).Returns("../../../../src/bin/Debug/net8.0/wwwroot");
            mockWebHostEnvironment.Setup(m => m.ContentRootPath).Returns("./data/");

            var MockLoggerDirect = Mock.Of<ILogger<IndexModel>>();
            JsonFileProductService productService;

            productService = new JsonFileProductService(mockWebHostEnvironment.Object);

            pageModel = new ReadModel(productService)
            {
            };
        }

        #endregion TestSetup

        /// <summary>
        /// When the product exists in the JSON data file, OnGet should return Page and set Product
        /// </summary>
        [Test]
        public void ReadModel_OnGet_ProductExists_ReturnsPageAndSetsProduct()
        {
            // Arrange - create temporary webroot with a products.json containing the test product
            var temp = Path.Combine(Path.GetTempPath(), "JsonTest_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.Combine(temp, "data"));

            var productsPath = Path.Combine(temp, "data", "products.json");
            File.WriteAllText(productsPath, "[ { \"Id\": \"read-test\", \"Title\": \"Read Test\" } ]");

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(m => m.WebRootPath).Returns(temp);
            mockEnv.Setup(m => m.EnvironmentName).Returns("Hosting:UnitTestEnvironment");

            var service = new JsonFileProductService(mockEnv.Object);
            var data = new ReadModel(service);

            // Act
            var result = data.OnGet("read-test");

            // Assert
            Assert.AreEqual(true, result is Microsoft.AspNetCore.Mvc.RazorPages.PageResult);
            Assert.AreNotEqual(null, data.Product);
            Assert.AreEqual("read-test", data.Product.Id);

            // Reset
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, true);
            }
        }

        /// <summary>
        /// When OnGet is called with an invalid/non-existent id, it should redirect to Index
        /// </summary>
        [Test]
        public void ReadModel_OnGet_InvalidId_RedirectsToIndex()
        {
            // Arrange
            var temp = Path.Combine(Path.GetTempPath(), "JsonTest_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.Combine(temp, "data"));
            var productsPath = Path.Combine(temp, "data", "products.json");
            File.WriteAllText(productsPath, "[]");

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(m => m.WebRootPath).Returns(temp);

            var service = new JsonFileProductService(mockEnv.Object);
            var data = new ReadModel(service);

            // Act
            var result = data.OnGet("no-such-id");

            // Assert
            Assert.AreEqual(true, result is Microsoft.AspNetCore.Mvc.RedirectToPageResult);

            // Reset
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, true);
            }
        }

        /// <summary>
        /// When the product is not present, OnGet should redirect to Index
        /// </summary>
        [Test]
        public void ReadModel_OnGet_ProductNotFound_RedirectsToIndex()
        {
            // Arrange
            var temp = Path.Combine(Path.GetTempPath(), "JsonTest_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.Combine(temp, "data"));
            var productsPath = Path.Combine(temp, "data", "products.json");
            File.WriteAllText(productsPath, "[]");

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(m => m.WebRootPath).Returns(temp);

            var service = new JsonFileProductService(mockEnv.Object);
            var data = new ReadModel(service);

            // Act
            var result = data.OnGet("no-such-id");

            // Assert
            Assert.AreEqual(true, result is Microsoft.AspNetCore.Mvc.RedirectToPageResult);
            var redirect = (Microsoft.AspNetCore.Mvc.RedirectToPageResult)result;
            Assert.AreEqual("./Index", redirect.PageName);

            // Reset
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, true);
            }
        }

        /// <summary>
        /// When the injected ProductService is null, OnGet should redirect to Index (fast-fail)
        /// </summary>
        [Test]
        public void ReadModel_OnGet_NullService_RedirectsToIndex()
        {
            // Arrange
            var data = new ReadModel(null);

            // Act
            var result = data.OnGet("anything");

            // Assert
            Assert.AreEqual(true, result is Microsoft.AspNetCore.Mvc.RedirectToPageResult);
            var redirect = (Microsoft.AspNetCore.Mvc.RedirectToPageResult)result;
            Assert.AreEqual("./Index", redirect.PageName);
        }

        /// <summary>
        /// When OnGet is called with an empty or whitespace id, it should redirect to Index.
        /// This covers the string.IsNullOrWhiteSpace(id) branch in OnGet.
        /// </summary>
        [Test]
        public void ReadModel_OnGet_EmptyOrWhitespaceId_RedirectsToIndex()
        {
            // Arrange
            var temp = Path.Combine(Path.GetTempPath(), "JsonTest_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.Combine(temp, "data"));
            var productsPath = Path.Combine(temp, "data", "products.json");
            // provide an empty array so the service can be constructed
            File.WriteAllText(productsPath, "[]");

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(m => m.WebRootPath).Returns(temp);

            var service = new JsonFileProductService(mockEnv.Object);
            var data = new ReadModel(service);

            // Act - empty string
            var resultEmpty = data.OnGet("");

            // Assert
            Assert.AreEqual(true, resultEmpty is Microsoft.AspNetCore.Mvc.RedirectToPageResult);
            var redirectEmpty = (Microsoft.AspNetCore.Mvc.RedirectToPageResult)resultEmpty;
            Assert.AreEqual("./Index", redirectEmpty.PageName);

            // Act - whitespace string
            var resultWhitespace = data.OnGet("   ");

            // Assert
            Assert.AreEqual(true, resultWhitespace is Microsoft.AspNetCore.Mvc.RedirectToPageResult);
            var redirectWhitespace = (Microsoft.AspNetCore.Mvc.RedirectToPageResult)resultWhitespace;
            Assert.AreEqual("./Index", redirectWhitespace.PageName);

            // Reset
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, true);
            }
        }

        /// <summary>
        /// When the products list contains null entries and entries with null Id,
        /// FindProductById should skip those and still find the valid product.
        /// This covers the branches where candidate == null and candidate.Id == null.
        /// </summary>
        [Test]
        public void ReadModel_OnGet_ListWithNullCandidates_FindsValidProduct()
        {
            // Arrange
            var temp = Path.Combine(Path.GetTempPath(), "JsonTest_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.Combine(temp, "data"));

            var productsPath = Path.Combine(temp, "data", "products.json");
            // JSON includes: null entry, entry with null Id, and the real product
            File.WriteAllText(productsPath, "[ null, { \"Id\": null, \"Title\": \"NoId\" }, { \"Id\": \"target\", \"Title\": \"Target\" } ]");

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(m => m.WebRootPath).Returns(temp);
            mockEnv.Setup(m => m.EnvironmentName).Returns("Hosting:UnitTestEnvironment");

            var service = new JsonFileProductService(mockEnv.Object);
            var data = new ReadModel(service);

            // Act
            var result = data.OnGet("target");

            // Assert
            Assert.AreEqual(true, result is Microsoft.AspNetCore.Mvc.RazorPages.PageResult);
            Assert.AreNotEqual(null, data.Product);
            Assert.AreEqual("target", data.Product.Id);

            // Reset
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, true);
            }
        }

        /// <summary>
        /// Directly invoke the private FindProductById via reflection and verify
        /// that when passed null/empty/whitespace id it returns null.
        /// This covers the early-return branch in FindProductById.
        /// </summary>
        [Test]
        public void ReadModel_FindProductById_NullOrWhitespaceId_ReturnsNull()
        {
            // Arrange
            var temp = Path.Combine(Path.GetTempPath(), "JsonTest_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.Combine(temp, "data"));
            var productsPath = Path.Combine(temp, "data", "products.json");
            File.WriteAllText(productsPath, "[]");

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(m => m.WebRootPath).Returns(temp);

            var service = new JsonFileProductService(mockEnv.Object);
            var data = new ReadModel(service);

            // Use reflection to get the private method
            var method = typeof(ReadModel).GetMethod("FindProductById", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.AreNotEqual(null, method, "Could not find FindProductById method via reflection");

            // Act & Assert - Invoke with null
            var resultNull = method.Invoke(data, new object[] { null });
            Assert.AreEqual(null, resultNull, "Expected null when id is null");

            // Act & Assert - Invoke with empty string
            var resultEmpty = method.Invoke(data, new object[] { string.Empty });
            Assert.AreEqual(null, resultEmpty, "Expected null when id is empty string");

            // Act & Assert - Invoke with whitespace
            var resultWhitespace = method.Invoke(data, new object[] { "   " });
            Assert.AreEqual(null, resultWhitespace, "Expected null when id is whitespace");

            // Reset
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, true);
            }
        }

        #region FindProductById
        [Test]
        public void FindProductById_CandidateWithDifferentId_ReturnsNull_WhenNoMatchingIdPresent()
        {
            // Arrange
            var temp = Path.Combine(Path.GetTempPath(), "JsonTest_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.Combine(temp, "data"));
            var productsPath = Path.Combine(temp, "data", "products.json");

            // Create a product with an Id that does NOT match the lookup value
            File.WriteAllText(productsPath, "[ { \"Id\": \"existing-id\", \"Title\": \"Existing\" } ]");

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(m => m.WebRootPath).Returns(temp);

            var service = new JsonFileProductService(mockEnv.Object);
            var data = new ReadModel(service);

            // Use reflection to get the private method
            var method = typeof(ReadModel).GetMethod("FindProductById", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.AreNotEqual(null, method, "Could not find FindProductById method via reflection");

            // Act - lookup an id that is not present
            var result = method.Invoke(data, new object[] { "different-id" });

            // Assert - should be null because no candidate matched
            Assert.AreEqual(null, result, "Expected FindProductById to return null when no candidate IDs match.");

            // Reset
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, true);
            }
        }

        [Test]
        /// <summary>
        /// Directly invokes FindProductById when the products array contains a mix of
        /// null entries, null-Id entries, non-matching and a later matching entry.
        /// Ensures the method skips invalid candidates and returns the later match.
        /// </summary>
        public void FindProductById_MixedCandidates_ReturnsMatchingLater()
        {
            // Arrange
            var temp = Path.Combine(Path.GetTempPath(), "JsonTest_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.Combine(temp, "data"));
            var productsPath = Path.Combine(temp, "data", "products.json");

            // JSON: null, null-Id, non-matching, then matching
            File.WriteAllText(productsPath, "[ null, { \"Id\": null }, { \"Id\": \"no\" }, { \"Id\": \"target\", \"Title\": \"T\" } ]");

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(m => m.WebRootPath).Returns(temp);

            var service = new JsonFileProductService(mockEnv.Object);
            var data = new ReadModel(service);

            var method = typeof(ReadModel).GetMethod("FindProductById", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method);

            // Act
            var result = method.Invoke(data, new object[] { "target" });

            // Assert
            Assert.IsNotNull(result);
            var returned = result as ContosoCrafts.WebSite.Models.ProductModel;
            Assert.IsNotNull(returned);
            Assert.AreEqual("target", returned.Id);

            // Cleanup
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, true);
            }
        }

        [Test]
        /// <summary>
        /// Constructs ReadModel with null ProductService and invokes FindProductById to exercise
        /// the products = System.Array.Empty<ProductModel>() branch. Expects null result.
        /// </summary>
        public void ReadModel_FindProductById_NullService_UsesEmptyArray_ReturnsNull()
        {
            // Arrange
            var model = new ReadModel(null);
            var method = typeof(ReadModel).GetMethod("FindProductById", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method);

            // Act
            var result = method.Invoke(model, new object[] { "any-id" });

            // Assert
            Assert.AreEqual(null, result);
        }
        #endregion
    }
}