using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using ContosoCrafts.WebSite.Pages.Product;
using ContosoCrafts.WebSite.Services;
using ContosoCrafts.WebSite.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UnitTests.Pages.Product
{
    /// <summary>
    /// Unit tests for UpdateModel page functionality.
    /// </summary>
    [TestFixture]
    public class UpdateModelTests
    {
        #region Helper Methods

        /// <summary>
        /// Creates a temporary web root folder with data and images subfolders for testing.
        /// </summary>
        private string CreateTempWebRoot(out string dataFile)
        {
            var root = Path.Combine(Path.GetTempPath(), "contoso_update_tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(root);

            var dataDir = Path.Combine(root, "data");
            Directory.CreateDirectory(dataDir);

            var imagesDir = Path.Combine(root, "images");
            Directory.CreateDirectory(imagesDir);

            dataFile = Path.Combine(dataDir, "products.json");

            return root;
        }

        #endregion

        #region OnGet Tests

        /// <summary>
        /// Tests that OnGet redirects to Index when ProductService is null.
        /// </summary>
        [Test]
        public void OnGet_ProductServiceIsNull_WhenCalled_ReturnsRedirectToIndex()
        {
            // Arrange
            var UpdateModel = new UpdateModel(null);

            // Act
            var Result = UpdateModel.OnGet("any-id");

            // Reset
            // (no external resources to clean)

            // Assert
            Assert.IsInstanceOf<RedirectToPageResult>(Result);
            var RedirectResult = (RedirectToPageResult)Result;
            Assert.AreEqual("./Index", RedirectResult.PageName);
        }

        /// <summary>
        /// Tests that OnGet redirects to Index when the requested product is not found.
        /// </summary>
        [Test]
        public void OnGet_ProductNotFound_WhenCalled_ReturnsRedirectToIndex()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot(out var DataFile);
            var JsonData = "[]";
            File.WriteAllText(DataFile, JsonData);

            var EnvironmentMock = new Mock<IWebHostEnvironment>();
            EnvironmentMock.SetupGet(e => e.WebRootPath).Returns(WebRoot);

            var Service = new JsonFileProductService(EnvironmentMock.Object);
            var UpdateModel = new UpdateModel(Service);

            // Act
            var Result = UpdateModel.OnGet("missing-id");

            // Reset
            Directory.Delete(WebRoot, true);

            // Assert
            Assert.IsInstanceOf<RedirectToPageResult>(Result);
        }

        /// <summary>
        /// Tests that OnGet returns the page and sets the Product when it exists.
        /// </summary>
        [Test]
        public void OnGet_ProductExists_WhenCalled_ReturnsPageAndSetsProduct()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot(out var DataFile);
            var Product = new ProductModel { Id = "u1", Title = "U1", Description = "d", Url = "u", Image = "/images/x.png", Location = "loc" };
            var JsonData = JsonSerializer.Serialize(new[] { Product });
            File.WriteAllText(DataFile, JsonData);

            var EnvironmentMock = new Mock<IWebHostEnvironment>();
            EnvironmentMock.SetupGet(e => e.WebRootPath).Returns(WebRoot);

            var Service = new JsonFileProductService(EnvironmentMock.Object);
            var UpdateModel = new UpdateModel(Service);

            // Act
            var Result = UpdateModel.OnGet("u1");

            // Reset
            Directory.Delete(WebRoot, true);

            // Assert
            Assert.IsInstanceOf<PageResult>(Result);
            Assert.IsNotNull(UpdateModel.Product);
            Assert.AreEqual("u1", UpdateModel.Product.Id);
        }

        #endregion

        #region OnPost Tests

        /// <summary>
        /// Tests that OnPost processes a new uploaded image, deletes the old one, and redirects.
        /// </summary>
        [Test]
        public void OnPost_ProductHasUpload_WhenCalled_ProcessesImageDeletesOldImageAndRedirects()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot(out var DataFile);

            var ProductId = "p3";
            var ExistingProduct = new ProductModel
            {
                Id = ProductId,
                Title = "Product Three",
                Description = "desc",
                Url = "url",
                Image = "/images/old.png",
                Location = "loc"
            };

            var JsonData = JsonSerializer.Serialize(new[] { ExistingProduct });
            File.WriteAllText(DataFile, JsonData);

            var ImagesFolder = Path.Combine(WebRoot, "images");
            Directory.CreateDirectory(ImagesFolder);
            var OldImagePath = Path.Combine(ImagesFolder, "old.png");
            File.WriteAllText(OldImagePath, "old");

            var EnvironmentMock = new Mock<IWebHostEnvironment>();
            EnvironmentMock.SetupGet(e => e.WebRootPath).Returns(WebRoot);

            var Service = new JsonFileProductService(EnvironmentMock.Object);
            var UpdateModel = new UpdateModel(Service);

            UpdateModel.Product = new ProductModel
            {
                Id = ProductId,
                Title = "Product Three",
                Description = "desc",
                Url = "url",
                Image = ExistingProduct.Image,
                Location = "loc"
            };

            var FileBytes = Encoding.UTF8.GetBytes("fakeimagecontent");
            var UploadStream = new MemoryStream(FileBytes);
            var FormFile = new FormFile(UploadStream, 0, FileBytes.Length, "Upload", "new.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            UpdateModel.Upload = FormFile;

            // Act
            var Result = UpdateModel.OnPost();

            // Reset
            Directory.Delete(WebRoot, true);

            // Assert
            Assert.IsInstanceOf<RedirectToPageResult>(Result);
            Assert.IsFalse(File.Exists(OldImagePath));

        }

        /// <summary>
        /// Tests that OnPost returns the page and adds a ModelState error when required fields are missing.
        /// </summary>
        [Test]
        public void OnPost_MissingRequiredFields_WhenCalled_ReturnsPageAndAddsModelError()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot(out var DataFile);
            var JsonData = "[]";
            File.WriteAllText(DataFile, JsonData);

            var EnvironmentMock = new Mock<IWebHostEnvironment>();
            EnvironmentMock.SetupGet(e => e.WebRootPath).Returns(WebRoot);

            var Service = new JsonFileProductService(EnvironmentMock.Object);
            var UpdateModel = new UpdateModel(Service);

            UpdateModel.Product = new ProductModel
            {
                Id = "no",
                Title = string.Empty,
                Description = string.Empty,
                Url = string.Empty,
                Image = string.Empty,
                Location = string.Empty
            };

            // Act
            var Result = UpdateModel.OnPost();

            // Reset
            Directory.Delete(WebRoot, true);

            // Assert
            Assert.IsInstanceOf<PageResult>(Result);
            Assert.IsFalse(UpdateModel.ModelState.IsValid);
            Assert.IsTrue(UpdateModel.ModelState.Values.SelectMany(v => v.Errors).Any());
        }

        /// <summary>
        /// Tests that OnPost redirects to Index when the product is missing during update.
        /// </summary>
        [Test]
        public void OnPost_ProductMissingDuringUpdate_WhenCalled_RedirectsToIndex()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot(out var DataFile);
            var JsonData = "[]"; // products.json empty so UpdateData will return false
            File.WriteAllText(DataFile, JsonData);

            var EnvironmentMock = new Mock<IWebHostEnvironment>();
            EnvironmentMock.SetupGet(e => e.WebRootPath).Returns(WebRoot);

            var Service = new JsonFileProductService(EnvironmentMock.Object);
            var UpdateModel = new UpdateModel(Service);

            UpdateModel.Product = new ProductModel
            {
                Id = "missing",
                Title = "T",
                Description = "D",
                Url = "U",
                Image = "/images/x.png",
                Location = "L"
            };

            // Act
            var Result = UpdateModel.OnPost();

            // Reset
            Directory.Delete(WebRoot, true);

            // Assert
            Assert.IsInstanceOf<RedirectToPageResult>(Result);
            var RedirectResult = (RedirectToPageResult)Result;
            Assert.AreEqual("./Index", RedirectResult.PageName);
        }

        #endregion
    }
}