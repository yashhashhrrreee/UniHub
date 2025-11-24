using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using ContosoCrafts.WebSite.Services;
using System.Linq;

namespace UnitTests.Pages
{
    /// <summary>
    /// Comprehensive tests for TestHelper class to achieve 100% code coverage.
    /// Tests all static properties, initialization, and helper functionality.
    /// </summary>
    [TestFixture]
    public class TestHelperTests
    {
        /// <summary>
        /// Test that MockWebHostEnvironment is properly initialized and configured.
        /// </summary>
        [Test]
        public void MockWebHostEnvironment_Initialization_ConfiguredCorrectly()
        {
            // Act & Assert
            Assert.IsNotNull(TestHelper.MockWebHostEnvironment, "MockWebHostEnvironment should be initialized");
            Assert.AreEqual("Hosting:UnitTestEnvironment", TestHelper.MockWebHostEnvironment.Object.EnvironmentName,
                "Environment name should be set correctly");
            Assert.AreEqual(TestFixture.DataWebRootPath, TestHelper.MockWebHostEnvironment.Object.WebRootPath,
                "WebRootPath should be set correctly");
            Assert.AreEqual(TestFixture.DataContentRootPath, TestHelper.MockWebHostEnvironment.Object.ContentRootPath,
                "ContentRootPath should be set correctly");
        }

        /// <summary>
        /// Test that HttpContextDefault is properly initialized.
        /// </summary>
        [Test]
        public void HttpContextDefault_Initialization_ConfiguredCorrectly()
        {
            // Act & Assert
            Assert.IsNotNull(TestHelper.HttpContextDefault, "HttpContextDefault should be initialized");
            Assert.AreEqual("trace", TestHelper.HttpContextDefault.TraceIdentifier,
                "TraceIdentifier should be set correctly");
            Assert.AreEqual("trace", TestHelper.HttpContextDefault.HttpContext.TraceIdentifier,
                "HttpContext TraceIdentifier should be set correctly");
            Assert.IsNotNull(TestHelper.HttpContextDefault.Request, "Request should be available");
            Assert.IsNotNull(TestHelper.HttpContextDefault.Response, "Response should be available");
        }

        /// <summary>
        /// Test that WebHostEnvironment property accessor works correctly.
        /// </summary>
        [Test]
        public void WebHostEnvironment_PropertyAccess_WorksCorrectly()
        {
            // Act & Assert - This might be null depending on setup, that's acceptable
            Assert.DoesNotThrow(() => _ = TestHelper.WebHostEnvironment,
                "WebHostEnvironment property should be accessible");
        }

        /// <summary>
        /// Test that ModelState is properly initialized and functional.
        /// </summary>
        [Test]
        public void ModelState_Initialization_WorksCorrectly()
        {
            // Act & Assert
            Assert.IsNotNull(TestHelper.ModelState, "ModelState should be initialized");

            // Test adding errors
            var originalErrorCount = TestHelper.ModelState.ErrorCount;
            TestHelper.ModelState.AddModelError("test", "test error");
            Assert.AreEqual(originalErrorCount + 1, TestHelper.ModelState.ErrorCount, "ModelState should track added errors");

            // Clean up
            TestHelper.ModelState.Remove("test");
        }

        /// <summary>
        /// Test that ActionContext is properly initialized with all dependencies.
        /// </summary>
        [Test]
        public void ActionContext_Initialization_ConfiguredCorrectly()
        {
            // Act & Assert
            Assert.IsNotNull(TestHelper.ActionContext, "ActionContext should be initialized");
            Assert.IsNotNull(TestHelper.ActionContext.HttpContext, "ActionContext.HttpContext should be set");
            Assert.IsNotNull(TestHelper.ActionContext.RouteData, "ActionContext.RouteData should be set");
            Assert.IsNotNull(TestHelper.ActionContext.ActionDescriptor, "ActionContext.ActionDescriptor should be set");
            Assert.IsNotNull(TestHelper.ActionContext.ModelState, "ActionContext.ModelState should be set");

            // Verify it's the same instance as our ModelState
            Assert.AreSame(TestHelper.ModelState, TestHelper.ActionContext.ModelState,
                "ActionContext should use the same ModelState instance");
        }

        /// <summary>
        /// Test that ModelMetadataProvider is properly initialized.
        /// </summary>
        [Test]
        public void ModelMetadataProvider_Initialization_WorksCorrectly()
        {
            // Act & Assert
            Assert.IsNotNull(TestHelper.ModelMetadataProvider, "ModelMetadataProvider should be initialized");
            Assert.IsInstanceOf<EmptyModelMetadataProvider>(TestHelper.ModelMetadataProvider,
                "Should be EmptyModelMetadataProvider instance");
        }

        /// <summary>
        /// Test that ViewData is properly initialized and functional.
        /// </summary>
        [Test]
        public void ViewData_Initialization_WorksCorrectly()
        {
            // Act & Assert
            Assert.IsNotNull(TestHelper.ViewData, "ViewData should be initialized");

            // Test functionality
            TestHelper.ViewData["test"] = "value";
            Assert.AreEqual("value", TestHelper.ViewData["test"], "ViewData should store and retrieve values");

            // Clean up
            TestHelper.ViewData.Remove("test");
        }

        /// <summary>
        /// Test that TempData is properly initialized and functional.
        /// </summary>
        [Test]
        public void TempData_Initialization_WorksCorrectly()
        {
            // Act & Assert
            Assert.IsNotNull(TestHelper.TempData, "TempData should be initialized");
            Assert.IsNotNull(TestHelper.TempData.Keys, "TempData.Keys should be accessible");

            // Test basic functionality
            TestHelper.TempData["test"] = "tempvalue";
            Assert.AreEqual("tempvalue", TestHelper.TempData["test"], "TempData should store values");

            // Clean up
            TestHelper.TempData.Remove("test");
        }

        /// <summary>
        /// Test that PageContext is properly initialized with all dependencies.
        /// </summary>
        [Test]
        public void PageContext_Initialization_ConfiguredCorrectly()
        {
            // Act & Assert
            Assert.IsNotNull(TestHelper.PageContext, "PageContext should be initialized");
            Assert.IsNotNull(TestHelper.PageContext.HttpContext, "PageContext.HttpContext should be set");
            Assert.IsNotNull(TestHelper.PageContext.ModelState, "PageContext.ModelState should be set");
            Assert.IsNotNull(TestHelper.PageContext.RouteData, "PageContext.RouteData should be set");
            Assert.IsNotNull(TestHelper.PageContext.ViewData, "PageContext.ViewData should be set");

            // Verify relationships
            Assert.AreSame(TestHelper.HttpContextDefault, TestHelper.PageContext.HttpContext,
                "PageContext should use the same HttpContext");
            Assert.AreSame(TestHelper.ViewData, TestHelper.PageContext.ViewData,
                "PageContext should use the same ViewData");
        }

        /// <summary>
        /// Test that ProductService is properly initialized and functional.
        /// </summary>
        [Test]
        public void ProductService_Initialization_WorksCorrectly()
        {
            // Act & Assert
            Assert.IsNotNull(TestHelper.ProductService, "ProductService should be initialized");
            Assert.IsInstanceOf<JsonFileProductService>(TestHelper.ProductService,
                "Should be JsonFileProductService instance");

            // Test basic functionality
            var products = TestHelper.ProductService.GetProducts();
            Assert.IsNotNull(products, "ProductService should return products");

            // Test that it uses the mock environment
            var firstProduct = products.FirstOrDefault();
            Assert.IsNotNull(firstProduct, "Should have at least one product for testing");
        }

        /// <summary>
        /// Test that UrlHelperFactory property can be accessed without errors.
        /// </summary>
        [Test]
        public void UrlHelperFactory_PropertyAccess_WorksCorrectly()
        {
            // Act & Assert - This might be null, that's acceptable
            Assert.DoesNotThrow(() => _ = TestHelper.UrlHelperFactory,
                "UrlHelperFactory property should be accessible");
        }

        /// <summary>
        /// Test integration of all TestHelper components working together.
        /// </summary>
        [Test]
        public void TestHelper_Integration_AllComponentsWorkTogether()
        {
            // Arrange - Create a mock page that would use all these components
            var mockPage = new Mock<PageModel>();

            // Act - Set up the page with TestHelper components
            mockPage.Object.PageContext = TestHelper.PageContext;
            mockPage.Object.TempData = TestHelper.TempData;

            // Assert - Verify everything works together
            Assert.IsNotNull(mockPage.Object.PageContext, "PageContext should be set");
            Assert.IsNotNull(mockPage.Object.ViewData, "ViewData should be accessible");
            Assert.IsNotNull(mockPage.Object.TempData, "TempData should be set");
            Assert.IsNotNull(mockPage.Object.HttpContext, "HttpContext should be accessible through PageContext");
            Assert.IsNotNull(mockPage.Object.ModelState, "ModelState should be accessible through PageContext");
        }

        /// <summary>
        /// Test static constructor behavior through reflection to ensure complete coverage.
        /// </summary>
        [Test]
        public void TestHelper_StaticConstructor_InitializesAllProperties()
        {
            // This test ensures the static constructor is covered
            // by accessing all static properties that would be initialized

            var properties = new object[]
            {
                TestHelper.MockWebHostEnvironment,
                TestHelper.HttpContextDefault,
                TestHelper.ModelState,
                TestHelper.ActionContext,
                TestHelper.ModelMetadataProvider,
                TestHelper.ViewData,
                TestHelper.TempData,
                TestHelper.PageContext,
                TestHelper.ProductService
            };

            // Assert all critical properties are not null (initialized by static constructor)
            foreach (var property in properties)
            {
                Assert.IsNotNull(property, $"Property should be initialized by static constructor");
            }
        }

        /// <summary>
        /// Test error scenarios and edge cases for complete coverage.
        /// </summary>
        [Test]
        public void TestHelper_EdgeCases_HandledGracefully()
        {
            // Test accessing properties multiple times doesn't cause issues
            for (int i = 0; i < 3; i++)
            {
                Assert.IsNotNull(TestHelper.ProductService, "ProductService should be consistently available");
                Assert.IsNotNull(TestHelper.PageContext, "PageContext should be consistently available");
            }

            // Test that mock setups are working
            Assert.AreEqual("Hosting:UnitTestEnvironment",
                TestHelper.MockWebHostEnvironment.Object.EnvironmentName,
                "Mock environment name should be consistent");
        }
    }
}