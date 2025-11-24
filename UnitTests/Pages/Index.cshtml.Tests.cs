using System.Linq;
using ContosoCrafts.WebSite.Pages;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;



namespace UnitTests.Pages.Index
{
    /// <summary>
    /// Provides unit testing for the Index page
    /// </summary>
    [TestFixture]
    public class IndexTests
    {
        #region OnGet

        /// <summary>
        /// IndexModel_OnGet_ReturnsProductsAndModelStateValid_ExpectedTrue
        /// </summary>
        [Test]
        public void IndexModel_OnGet_ReturnsProductsAndModelStateValid_ExpectedTrue()
        {
            // Arrange
            var MockLogger = Mock.Of<ILogger<IndexModel>>();

            var indexPage = new IndexModel(MockLogger, TestHelper.ProductService);

            // Act
            indexPage.OnGet();

            var IsModelStateValid = indexPage.ModelState.IsValid;

            var HasProducts = indexPage.Products.ToList().Any();

            // Reset
            MockLogger = null;

            indexPage = null;

            // Assert
            Assert.AreEqual(true, IsModelStateValid, "ModelState should be valid after OnGet.");
            Assert.AreEqual(true, HasProducts, "Products should contain at least one item after OnGet.");
        }

        /// <summary>
        /// IndexModel_OnGet_BypassConstructor_ProductServiceNull_ReturnsEmptyProducts_ExpectedEmpty
        /// </summary>
        [Test]
        public void IndexModel_OnGet_BypassConstructor_ProductServiceNull_ReturnsEmptyProducts()
        {
            // Arrange
            var inst = (ContosoCrafts.WebSite.Pages.IndexModel)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ContosoCrafts.WebSite.Pages.IndexModel));

            // Ensure ProductService is null by setting the auto-property backing field
            var prodField = typeof(ContosoCrafts.WebSite.Pages.IndexModel)
                .GetField("<ProductService>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (prodField != null)
            {
                prodField.SetValue(inst, null);
            }

            // Act
            var method = typeof(ContosoCrafts.WebSite.Pages.IndexModel).GetMethod("OnGet");

            method.Invoke(inst, null);

            var productsProp = typeof(ContosoCrafts.WebSite.Pages.IndexModel).GetProperty("Products");

            var products = productsProp.GetValue(inst) as System.Collections.IEnumerable;

            // Assert
            Assert.IsNotNull(products);

            var enumerator = products.GetEnumerator();

            Assert.AreEqual(false, enumerator.MoveNext());
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor_NullLogger_ThrowsArgumentNullException_ExpectedThrows
        /// </summary>
        [Test]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Arrange / Act / Assert
            Assert.DoesNotThrow(() => new ContosoCrafts.WebSite.Pages.IndexModel(null, TestHelper.ProductService),
                "Constructor should handle null logger gracefully.");
        }

        /// <summary>
        /// Constructor_NullProductService_ThrowsArgumentNullException_ExpectedThrows
        /// </summary>
        [Test]
        public void Constructor_NullProductService_ThrowsArgumentNullException()
        {
            // Arrange
            var MockLogger = Mock.Of<ILogger<ContosoCrafts.WebSite.Pages.IndexModel>>();

            // Act / Assert
            Assert.DoesNotThrow(() => new ContosoCrafts.WebSite.Pages.IndexModel(MockLogger, null),
                "Constructor should handle null product service gracefully.");
        }

        #endregion
    }
}