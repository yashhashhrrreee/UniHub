using ContosoCrafts.WebSite.Pages.Product;
using ContosoCrafts.WebSite.Models;
using ContosoCrafts.WebSite.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.Json;
using System.IO;

namespace UnitTests.Pages.Product
{
    /// <summary>
    /// Unit tests for the <see cref="IndexModel"/> page model.
    /// Test class name follows the pattern: ClassName + "Test".
    /// </summary>
    public class IndexModelTest
    {
        /// <summary>
        /// Tests that TitleContains returns false when product Title is null.
        /// </summary>
        [Test]
        public void IndexModel_TitleContains_NullTitle_ReturnsFalse()
        {
            // Arrange
            var data = new IndexModel(null);
            var prod = new ProductModel { Title = null };

            var method = typeof(IndexModel).GetMethod("TitleContains", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = (bool)method.Invoke(data, new object[] { prod, "term" });

            // Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        public void TitleContains_ProductTitleNull_ReturnsFalse()
        {
            var model = new IndexModel(null);

            var prod = new ProductModel { Id = "t-null", Title = null };

            var method = typeof(IndexModel).GetMethod("TitleContains", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var args = new object[] { prod, "term" };

            var result = (bool)method.Invoke(model, args);

            Assert.AreEqual(false, result);
        }

        [Test]
        public void DescriptionContains_ProductDescriptionNull_ReturnsFalse()
        {
            var model = new IndexModel(null);

            var prod = new ProductModel { Id = "d-null", Description = null };

            var method = typeof(IndexModel).GetMethod("DescriptionContains", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var args = new object[] { prod, "term" };

            var result = (bool)method.Invoke(model, args);

            Assert.AreEqual(false, result);
        }
        /// <summary>
        /// Tests that DescriptionContains returns false when Product is null.
        /// </summary>
        [Test]
        public void IndexModel_TitleContains_NullProduct_ReturnsFalse()
        {
            // Arrange
            var data = new IndexModel(null);

            var method = typeof(IndexModel).GetMethod("TitleContains", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = (bool)method.Invoke(data, new object[] { null, "term" });

            // Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        public void TitleContains_TermNullOrWhitespace_ReturnsFalse()
        {
            var model = new IndexModel(null);
            var prod = new ProductModel { Id = "t1", Title = "SomeTitle" };

            var method = typeof(IndexModel).GetMethod("TitleContains", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // null term
            var resultNull = (bool)method.Invoke(model, new object[] { prod, null });
            Assert.AreEqual(false, resultNull);

            // empty term
            var resultEmpty = (bool)method.Invoke(model, new object[] { prod, string.Empty });
            Assert.AreEqual(false, resultEmpty);

            // whitespace term
            var resultSpace = (bool)method.Invoke(model, new object[] { prod, "   " });
            Assert.AreEqual(false, resultSpace);
        }

        [Test]
        public void IndexModel_DescriptionContains_NullProduct_ReturnsFalse()
        {
            // Arrange
            var data = new IndexModel(null);
            var method = typeof(IndexModel).GetMethod("DescriptionContains", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = (bool)method.Invoke(data, new object[] { null, "term" });

            // Assert
            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests that DescriptionContains returns false when product Description is null.
        /// </summary>
        [Test]
        public void IndexModel_DescriptionContains_NullDescription_ReturnsFalse()
        {
            // Arrange
            var data = new IndexModel(null);
            var prod = new ProductModel { Description = null };

            var method = typeof(IndexModel).GetMethod("DescriptionContains", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = (bool)method.Invoke(data, new object[] { prod, "term" });

            // Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        public void DescriptionContains_TermNullOrWhitespace_ReturnsFalse()
        {
            var model = new IndexModel(null);
            var prod = new ProductModel { Id = "d1", Description = "SomeDescription" };

            var method = typeof(IndexModel).GetMethod("DescriptionContains", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // null term
            var resultNull = (bool)method.Invoke(model, new object[] { prod, null });
            Assert.AreEqual(false, resultNull);

            // empty term
            var resultEmpty = (bool)method.Invoke(model, new object[] { prod, string.Empty });
            Assert.AreEqual(false, resultEmpty);

            // whitespace term
            var resultSpace = (bool)method.Invoke(model, new object[] { prod, "   " });
            Assert.AreEqual(false, resultSpace);
        }

        /// <summary>
        /// Tests that OnGet correctly filters products by TypeOfUniversity enum string.
        /// </summary>
        [Test]
        public void IndexModel_OnGet_TypeFilter_ParsesAndFilters()
        {
            // Arrange
            var p1 = new ProductModel { Id = "1", Title = "A", Description = "a", TypeOfUniversity = ProductTypeEnum.Public };
            var p2 = new ProductModel { Id = "2", Title = "B", Description = "b", TypeOfUniversity = ProductTypeEnum.Private };

            var list = new[] { p1, p2 };
            var file = System.IO.Path.Combine(TestFixture.DataWebRootPath, "data", "products.json");
            System.IO.File.WriteAllText(file, System.Text.Json.JsonSerializer.Serialize(list));

            var env = new Moq.Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns(TestFixture.DataWebRootPath);
            var service = new ContosoCrafts.WebSite.Services.JsonFileProductService(env.Object);

            var data = new IndexModel(service)
            {
                TypeFilter = "Public"
            };

            // Act
            data.OnGet();
            var results = data.Products.ToList();

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("1", results[0].Id);
        }
        #region TestSetup
        public static IndexModel pageModel;
        /// <summary>
        /// Initialize of Test
        /// </summary>
        [SetUp]
        public void TestInitialize()
        {
            pageModel = new IndexModel(TestHelper.ProductService)
            {
            };
        }

        #endregion TestSetup
        /// <summary>
        /// Checking whether product user want is there in result or not.
        /// </summary>
        #region OnGet

        [Test]
        public void IndexModel_OnGet_Valid_NoSearch_ReturnsProducts_MatchingServiceCount()
        {
            // Arrange

            // Act
            pageModel.OnGet();

            // Reset (capture values before clearing shared/static state)
            var modelStateValid = pageModel.ModelState.IsValid;
            var productsNotNull = pageModel.Products != null;
            var actualCount = pageModel.Products.Count();
            var expectedCount = TestHelper.ProductService.GetProducts().Count();
            pageModel = null;

            // Assert 
            Assert.AreEqual(true, modelStateValid);
            Assert.AreEqual(true, productsNotNull);
            Assert.AreEqual(expectedCount, actualCount);
        }

        [Test]
        public void IndexModel_SearchTerm_Set_ValidString_SetsPropertyValue()
        {
            // Arrange
            var data = new IndexModel(TestHelper.ProductService);

            // Act
            data.SearchTerm = "abc";
            var result = data.SearchTerm;

            // Reset
            data = null;

            // Assert
            Assert.AreEqual("abc", result);
        }

        [Test]
        public void IndexModel_OnGet_ProductServiceNull_ReturnsEmptyProducts()
        {
            // Arrange
            var data = new IndexModel(null);

            // Act
            data.OnGet();
            var result = data.Products;

            // Reset
            data = null;

            // Assert
            Assert.AreNotEqual(null, result);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void IndexModel_OnGet_WithSearchTerm_TitleOrDescription_ReturnsFilteredProducts()
        {
            // Arrange
            var p1 = new ProductModel { Id = "1", Title = "matchme", Description = "no" };
            var p2 = new ProductModel { Id = "2", Title = "no", Description = "matchme too" };
            var p3 = new ProductModel { Id = "3", Title = "matchme", Description = "matchme too" };
            var p4 = new ProductModel { Id = "4", Title = "nomatch", Description = "none" };

            var env = new Moq.Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns(TestFixture.DataWebRootPath);
            var tmpService = new ContosoCrafts.WebSite.Services.JsonFileProductService(env.Object);

            var list = new[] { p1, p2, p3, p4 };
            var file = System.IO.Path.Combine(TestFixture.DataWebRootPath, "data", "products.json");
            System.IO.File.WriteAllText(file, System.Text.Json.JsonSerializer.Serialize(list));

            var data = new IndexModel(tmpService)
            {
                SearchTerm = "matchme"
            };

            // Act
            data.OnGet();
            var result = data.Products.ToList();

            // Reset
            data = null;

            // Assert
            Assert.AreEqual(3, result.Count);
            CollectionAssert.AreEquivalent(new[] { "1", "2", "3" }, result.Select(p => p.Id).ToArray());
        }

        [Test]
        public void IndexModel_TitleContains_TermNotPresent_ReturnsFalse()
        {
            // Arrange
            var data = new IndexModel(null);
            var prod = new ProductModel { Title = "Hello World" };

            var method = typeof(IndexModel).GetMethod("TitleContains", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = (bool)method.Invoke(data, new object[] { prod, "nomatch" });

            // Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        public void IndexModel_DescriptionContains_TermNotPresent_ReturnsFalse()
        {
            // Arrange
            var data = new IndexModel(null);
            var prod = new ProductModel { Description = "Something else" };

            var method = typeof(IndexModel).GetMethod("DescriptionContains", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = (bool)method.Invoke(data, new object[] { prod, "nomatch" });

            // Assert
            Assert.AreEqual(false, result);
        }
        #endregion OnGet

        [Test]
        public void OnGet_TypeFilter_Invalid_DoesNotFilter()
        {
            var p1 = new ProductModel { Id = "1", Title = "A", Description = "a", TypeOfUniversity = ProductTypeEnum.Public };
            var p2 = new ProductModel { Id = "2", Title = "B", Description = "b", TypeOfUniversity = ProductTypeEnum.Private };

            var list = new[] { p1, p2 };

            var webRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "idx_test", System.Guid.NewGuid().ToString());
            Directory.CreateDirectory(webRoot);
            var dataDir = Path.Combine(webRoot, "data");
            Directory.CreateDirectory(dataDir);
            var file = Path.Combine(dataDir, "products.json");
            File.WriteAllText(file, JsonSerializer.Serialize(list));

            var env = new Moq.Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns(webRoot);

            var service = new JsonFileProductService(env.Object);
            var model = new IndexModel(service)
            {
                TypeFilter = "NotAnEnumValue"
            };

            model.OnGet();

            var results = model.Products.ToList();

            Assert.AreEqual(2, results.Count);

            // cleanup
            Directory.Delete(webRoot, true);
        }

        /// <summary>
        /// Test TitleContains with valid match - positive case
        /// </summary>
        [Test]
        public void TitleContains_ValidMatch_ReturnsTrue()
        {
            // Arrange
            var model = new IndexModel(null);
            var product = new ProductModel { Title = "Harvard University" };
            var method = typeof(IndexModel).GetMethod("TitleContains", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = (bool)method.Invoke(model, new object[] { product, "Harvard" });

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test DescriptionContains with valid match - positive case
        /// </summary>
        [Test]
        public void DescriptionContains_ValidMatch_ReturnsTrue()
        {
            // Arrange
            var model = new IndexModel(null);
            var product = new ProductModel { Description = "Private research university" };
            var method = typeof(IndexModel).GetMethod("DescriptionContains", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = (bool)method.Invoke(model, new object[] { product, "research" });

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test GetFlagClass method for all university types
        /// </summary>
        [Test]
        public void GetFlagClass_AllUniversityTypes_ReturnsCorrectClasses()
        {
            // Arrange
            var model = new IndexModel(null);

            // Act & Assert
            Assert.AreEqual("flag-public", model.GetFlagClass(ProductTypeEnum.Public));
            Assert.AreEqual("flag-private", model.GetFlagClass(ProductTypeEnum.Private));
            Assert.AreEqual("flag-online", model.GetFlagClass(ProductTypeEnum.Online));
            Assert.AreEqual("flag-community", model.GetFlagClass(ProductTypeEnum.Community));
            Assert.AreEqual("flag-other", model.GetFlagClass(ProductTypeEnum.Other));
            Assert.AreEqual("flag-undefined", model.GetFlagClass(ProductTypeEnum.Undefined));
        }

        /// <summary>
        /// Test TypeFilter property setter and getter
        /// </summary>
        [Test]
        public void TypeFilter_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var model = new IndexModel(null);

            // Act
            model.TypeFilter = "Public";

            // Assert
            Assert.AreEqual("Public", model.TypeFilter);
        }

        /// <summary>
        /// Test case-insensitive search in both title and description
        /// </summary>
        [Test]
        public void TitleContains_CaseInsensitive_ReturnsTrue()
        {
            // Arrange
            var model = new IndexModel(null);
            var product = new ProductModel { Title = "Harvard University" };
            var method = typeof(IndexModel).GetMethod("TitleContains", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = (bool)method.Invoke(model, new object[] { product, "HARVARD" });

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test case-insensitive search in description
        /// </summary>
        [Test]
        public void DescriptionContains_CaseInsensitive_ReturnsTrue()
        {
            // Arrange
            var model = new IndexModel(null);
            var product = new ProductModel { Description = "Private research university" };
            var method = typeof(IndexModel).GetMethod("DescriptionContains", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = (bool)method.Invoke(model, new object[] { product, "RESEARCH" });

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test combined search term and type filter
        /// </summary>
        [Test]
        public void OnGet_CombinedSearchAndType_FiltersCorrectly()
        {
            // Arrange
            var p1 = new ProductModel { Id = "1", Title = "Harvard University", Description = "Private research", TypeOfUniversity = ProductTypeEnum.Private };
            var p2 = new ProductModel { Id = "2", Title = "MIT", Description = "Technology institute", TypeOfUniversity = ProductTypeEnum.Private };
            var p3 = new ProductModel { Id = "3", Title = "UC Berkeley", Description = "Public university", TypeOfUniversity = ProductTypeEnum.Public };

            var list = new[] { p1, p2, p3 };
            var webRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "idx_test", System.Guid.NewGuid().ToString());
            Directory.CreateDirectory(webRoot);
            var dataDir = Path.Combine(webRoot, "data");
            Directory.CreateDirectory(dataDir);
            var file = Path.Combine(dataDir, "products.json");
            File.WriteAllText(file, JsonSerializer.Serialize(list));

            var env = new Moq.Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns(webRoot);

            var service = new JsonFileProductService(env.Object);
            var model = new IndexModel(service)
            {
                SearchTerm = "University",
                TypeFilter = "Private"
            };

            // Act
            model.OnGet();
            var results = model.Products.ToList();

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("1", results[0].Id);

            // Cleanup
            Directory.Delete(webRoot, true);
        }
    }
}