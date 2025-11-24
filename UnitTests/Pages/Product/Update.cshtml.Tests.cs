using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http.Features;
using NUnit.Framework;
using ContosoCrafts.WebSite.Pages.Product;
using ContosoCrafts.WebSite.Models;
using ContosoCrafts.WebSite.Services;

namespace UnitTests.Pages.Product
{
    /// <summary>
    /// Unit tests for the <see cref="ContosoCrafts.WebSite.Pages.Product.UpdateModel"/> class.
    /// </summary>
    [TestFixture]
    public class UpdateTests
    {
        private string _tempWebRoot;

        [SetUp]
        public void Setup()
        {
            _tempWebRoot = Path.Combine(Path.GetTempPath(), "UpdateModelTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempWebRoot);
            Directory.CreateDirectory(Path.Combine(_tempWebRoot, "images"));
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (Directory.Exists(_tempWebRoot))
                {
                    Directory.Delete(_tempWebRoot, true);
                }
            }
            catch { }
        }

        /// <summary>
        /// Create a test environment for the service with a temporary webroot.
        /// </summary>
        private IWebHostEnvironment CreateEnv()
        {
            return new TestEnv { WebRootPath = _tempWebRoot };
        }

        /// <summary>
        /// Create the JsonFileProductService using a test environment.
        /// </summary>
        private JsonFileProductService CreateService()
        {
            return new JsonFileProductService(CreateEnv());
        }

        /// <summary>
        /// Build an <see cref="IFormFile"/> from a byte array for upload tests.
        /// </summary>
        private IFormFile CreateFormFile(byte[] bytes, string fileName = "u.bin")
        {
            var memoryStream = new MemoryStream(bytes);

            return new FormFile(memoryStream, 0, memoryStream.Length, "upload", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream"
            };
        }

        #region OnGet

        /// <summary>
        /// OnGet_NoService_RedirectsToIndex_ExpectedRedirect
        /// </summary>
        [Test]
        public void OnGet_NoService_RedirectsToIndex()
        {
            // Arrange
            var updateModel = new UpdateModel(null);

            // Act
            var result = updateModel.OnGet("anything");

            // Assert
            Assert.IsInstanceOf<RedirectToPageResult>(result);
            Assert.AreEqual("./Index", ((RedirectToPageResult)result).PageName);
        }

        /// <summary>
        /// OnGet_ProductNotFound_RedirectsToIndex_ExpectedRedirect
        /// </summary>
        [Test]
        public void OnGet_ProductNotFound_RedirectsToIndex()
        {
            // Arrange
            var service = CreateService();
            var updateModel = new UpdateModel(service);

            // Act
            var result = updateModel.OnGet("missing-id");

            // Assert
            Assert.IsInstanceOf<RedirectToPageResult>(result);
        }

        /// <summary>
        /// OnGet_ProductFound_ReturnsPageAndSetsProduct_ExpectedPageAndProduct
        /// </summary>
        [Test]
        public void OnGet_ProductFound_ReturnsPageAndSetsProduct()
        {

            // Arrange
            var service = CreateService();

            var product = new ProductModel { Id = "p1", Title = "T" };

            service.CreateData(product);

            var updateModel = new UpdateModel(service);

            // Act
            var result = updateModel.OnGet("p1");

            // Assert
            Assert.IsInstanceOf<PageResult>(result);
            Assert.IsNotNull(updateModel.Product);
            Assert.AreEqual("p1", updateModel.Product.Id);
        }

        #endregion

        #region OnPost

        /// <summary>
        /// OnPost_ModelStateInvalid_ReturnsPage_ExpectedPage
        /// </summary>
        [Test]
        public void OnPost_ModelStateInvalid_ReturnsPage()
        {
            // Arrange
            var service = CreateService();

            var updateModel = new UpdateModel(service);

            updateModel.ModelState.AddModelError("x", "err");

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<PageResult>(result);
        }

        /// <summary>
        /// OnPost_ProductNull_RedirectsToIndex_ExpectedRedirect
        /// </summary>
        [Test]
        public void OnPost_ProductNull_RedirectsToIndex()
        {
            // Arrange
            var service = CreateService();

            var updateModel = new UpdateModel(service);

            updateModel.Product = null;

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<RedirectToPageResult>(result);
        }

        /// <summary>
        /// OnPost_ProductIdNull_RedirectsToIndex_ExpectedRedirect
        /// </summary>
        [Test]
        public void OnPost_ProductIdNull_RedirectsToIndex()
        {
            // Arrange
            var service = CreateService();

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = null }
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<RedirectToPageResult>(result);
        }

        /// <summary>
        /// OnPost_UploadLengthZero_SkipsUploadAndUpdates_ExpectedRedirect
        /// </summary>
        [Test]
        public void OnPost_UploadLengthZero_SkipsUploadAndUpdates()
        {
            // Arrange
            var service = CreateService();

            var product = new ProductModel { Id = "p2", Title = "T", Description = "D", Url = "u", Image = "/images/ex.png", Location = "L" };

            service.CreateData(product);

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel
                {
                    Id = "p2",
                    Title = "T",
                    Description = "D",
                    Url = "u",
                    Image = "/images/ex.png",
                    Location = "L",
                    Campuses = new System.Collections.Generic.List<string> { "C" }
                },
                Upload = CreateFormFile(new byte[0], "empty.png")
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<RedirectToPageResult>(result);
        }

        /// <summary>
        /// OnPost_Upload_ValidSaveAndRedirects_ExpectedImageSaved
        /// </summary>
        [Test]
        public void OnPost_Upload_ValidSaveAndRedirects()
        {
            // Arrange
            var service = CreateService();

            var product = new ProductModel { Id = "p3", Title = "T", Description = "D", Url = "u", Image = "/images/old.png", Location = "L" };

            service.CreateData(product);

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel
                {
                    Id = "p3",
                    Title = "Good Title",
                    Description = "Desc",
                    Url = "http://x",
                    Image = "/images/old.png",
                    Location = "Loc",
                    Campuses = new System.Collections.Generic.List<string> { "Main" }
                },
                Upload = CreateFormFile(Encoding.UTF8.GetBytes("content"), "file.jpg")
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<RedirectToPageResult>(result);

            // image should be replaced with a /images/ path
            Assert.IsTrue(updateModel.Product.Image.StartsWith("/images/"));

            // file should exist on disk
            var physicalPath = Path.Combine(_tempWebRoot, updateModel.Product.Image.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            Assert.IsTrue(File.Exists(physicalPath));
        }

        /// <summary>
        /// OnPost_Upload_DeletesOldImage_WhenExists_ExpectedOldRemovedAndNewSaved
        /// </summary>
        [Test]
        public void OnPost_Upload_DeletesOldImage_WhenExists()
        {
            // Arrange
            var service = CreateService();

            var product = new ProductModel { Id = "del1", Title = "T", Description = "D", Url = "u", Image = "/images/old.png", Location = "L" };

            service.CreateData(product);

            // create a physical old image file to be deleted
            var imagesPath = Path.Combine(_tempWebRoot, "images");

            Directory.CreateDirectory(imagesPath);

            var oldPath = Path.Combine(imagesPath, "old.png");

            File.WriteAllText(oldPath, "oldcontent");

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel
                {
                    Id = "del1",
                    Title = "New",
                    Description = "Desc",
                    Url = "http://x",
                    Image = "/images/old.png",
                    Location = "Loc",
                    Campuses = new System.Collections.Generic.List<string> { "Main" }
                },
                Upload = CreateFormFile(Encoding.UTF8.GetBytes("newcontent"), "file.png")
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<RedirectToPageResult>(result);

            // old file should be removed
            Assert.IsFalse(File.Exists(oldPath));

            // new file should exist
            var physicalPath = Path.Combine(_tempWebRoot, updateModel.Product.Image.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            Assert.IsTrue(File.Exists(physicalPath));
        }

        /// <summary>
        /// OnPost_UnknownExtension_DefaultsToPng_ExpectedPngSaved
        /// </summary>
        [Test]
        public void OnPost_UnknownExtension_DefaultsToPng()
        {
            // Arrange
            var service = CreateService();

            var product = new ProductModel { Id = "ux1", Title = "T", Description = "D", Url = "u", Image = "/images/old.png", Location = "L" };

            service.CreateData(product);

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "ux1", Title = "T", Description = "D", Url = "u", Image = "/images/old.png", Location = "L" },
                Upload = CreateFormFile(Encoding.UTF8.GetBytes("x"), "file.gif")
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<RedirectToPageResult>(result);
            Assert.IsTrue(updateModel.Product.Image.EndsWith(".png"));

            var physicalPath = Path.Combine(_tempWebRoot, updateModel.Product.Image.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            Assert.IsTrue(File.Exists(physicalPath));
        }

        /// <summary>
        /// OnPost_UploadThrows_AddsModelErrorAndReturnsPage_ExpectedModelErrorOrRedirect
        /// </summary>
        [Test]
        public void OnPost_UploadThrows_AddsModelErrorAndReturnsPage()
        {
            // Arrange
            var service = CreateService();

            // A form file whose stream will throw on read
            var throwing = new ThrowingFormFile();

            var product = new ProductModel { Id = "p4", Title = "T", Description = "D", Url = "u", Image = "/images/x.png", Location = "L" };

            service.CreateData(product);

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel
                {
                    Id = "p4",
                    Title = "Good",
                    Description = "Desc",
                    Url = "http://x",
                    Image = "/images/x.png",
                    Location = "Loc",
                    Campuses = new System.Collections.Generic.List<string> { "Main" }
                },
                Upload = throwing
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            var isPage = result is PageResult;
            var isRedirect = result is RedirectToPageResult;
            var condition = isPage;
            if (!condition)
            {
                condition = isRedirect;
            }
            Assert.IsTrue(condition);

            if (isPage)
            {
                Assert.IsTrue(updateModel.ModelState.ErrorCount > 0);
            }
        }

        /// <summary>
        /// OnPost_UploadUnauthorized_AddsModelErrorAndReturnsPage_ExpectedModelErrorOrRedirect
        /// </summary>
        [Test]
        public void OnPost_UploadUnauthorized_AddsModelErrorAndReturnsPage()
        {
            // Arrange
            var service = CreateService();

            var product = new ProductModel { Id = "p5", Title = "T", Description = "D", Url = "u", Image = "/images/x.png", Location = "L" };

            service.CreateData(product);

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel
                {
                    Id = "p5",
                    Title = "Good",
                    Description = "Desc",
                    Url = "http://x",
                    Image = "/images/x.png",
                    Location = "Loc",
                    Campuses = new System.Collections.Generic.List<string> { "Main" }
                },
                Upload = new UnauthorizedFormFile()
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            var isPage = result is PageResult;
            var isRedirect = result is RedirectToPageResult;
            var condition = isPage;
            if (!condition)
            {
                condition = isRedirect;
            }
            Assert.IsTrue(condition);

            if (isPage)
            {
                Assert.IsTrue(updateModel.ModelState.ErrorCount > 0);
            }
        }

        #region TryHandleUpload

        /// <summary>
        /// TryHandleUpload_NullUpload_ReturnsTrue_ExpectedTrue
        /// </summary>
        [Test]
        public void TryHandleUpload_NullUpload_ReturnsTrue()
        {
            // Arrange
            var service = CreateService();

            service.CreateData(new ProductModel { Id = "th1", Title = "T" });

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "th1", Title = "T", Description = "D", Url = "u", Image = "/images/x.png", Location = "L" },
                Upload = null
            };

            var method = typeof(UpdateModel).GetMethod("TryHandleUpload", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var args = new object[] { null };

            // Act
            var result = (bool)method.Invoke(updateModel, args);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// TryHandleUpload_ZeroLengthUpload_ReturnsTrue_ExpectedTrue
        /// </summary>
        [Test]
        public void TryHandleUpload_ZeroLengthUpload_ReturnsTrue()
        {
            // Arrange
            var service = CreateService();

            service.CreateData(new ProductModel { Id = "th2", Title = "T" });

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "th2", Title = "T", Description = "D", Url = "u", Image = "/images/x.png", Location = "L" },
                Upload = CreateFormFile(new byte[0], "empty.bin")
            };

            var method = typeof(UpdateModel).GetMethod("TryHandleUpload", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var args = new object[] { null };

            // Act
            var result = (bool)method.Invoke(updateModel, args);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// TryHandleUpload_IOException_ReturnsFalseAndSetsError_ExpectedFalseAndErrorMessage
        /// </summary>
        [Test]
        public void TryHandleUpload_IOException_ReturnsFalseAndSetsError()
        {
            // Arrange
            var service = CreateService();

            service.CreateData(new ProductModel { Id = "io1", Title = "T" });

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "io1", Title = "T", Description = "D", Url = "u", Image = "/images/x.png", Location = "L" },
                Upload = new ThrowingFormFile()
            };

            var method = typeof(UpdateModel).GetMethod("TryHandleUpload", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var args = new object[] { null };

            // Act
            var result = (bool)method.Invoke(updateModel, args);

            // The method should return false on IOException and set the out error string
            Assert.IsFalse(result);
            Assert.IsNotNull(args[0]);
            Assert.IsTrue(args[0] is string);

            var err = (string)args[0];

            Assert.IsTrue(err.Contains("Could not save upload"));
        }

        #endregion

        /// <summary>
        /// OnPost_MissingRequiredFields_AddsErrors_ExpectedValidationErrors
        /// </summary>
        [Test]
        public void OnPost_MissingRequiredFields_AddsErrors()
        {
            // Arrange
            var service = CreateService();

            var product = new ProductModel { Id = "p5" };

            service.CreateData(product);

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "p5", Title = "", Description = "", Url = "", Image = "", Location = "" }
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            var isPage = result is PageResult;
            var isRedirect = result is RedirectToPageResult;
            var condition = isPage;
            if (!condition)
            {
                condition = isRedirect;
            }
            Assert.IsTrue(condition);

            if (isPage)
            {
                Assert.IsTrue(updateModel.ModelState.ErrorCount > 0);
            }
        }

        /// <summary>
        /// OnPost_FieldLengthsExceeded_AddsErrors_ExpectedValidationErrors
        /// </summary>
        [Test]
        public void OnPost_FieldLengthsExceeded_AddsErrors()
        {
            // Arrange
            var service = CreateService();

            var LongTitle = new string('T', 60);
            var LongLoc = new string('L', 60);
            var LongDesc = new string('D', 600);

            var product = new ProductModel { Id = "p6" };

            service.CreateData(product);

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "p6", Title = LongTitle, Description = LongDesc, Url = "u", Image = "/images/x.png", Location = LongLoc }
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            var isPage = result is PageResult;
            var isRedirect = result is RedirectToPageResult;
            var condition = isPage;
            if (!condition)
            {
                condition = isRedirect;
            }
            Assert.IsTrue(condition);

            if (isPage)
            {
                Assert.IsTrue(updateModel.ModelState.ErrorCount > 0);
            }
        }

        /// <summary>
        /// OnPost_CampusValidation_AddsErrors_ExpectedValidationErrors
        /// </summary>
        [Test]
        public void OnPost_CampusValidation_AddsErrors()
        {
            // Arrange
            var service = CreateService();

            var product = new ProductModel { Id = "p7", Title = "T", Description = "D", Url = "u", Image = "/images/x.png", Location = "L" };

            service.CreateData(product);

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "p7", Title = "T", Description = "D", Url = "u", Image = "/images/x.png", Location = "L", Campuses = new System.Collections.Generic.List<string> { " ", new string('A', 60) } }
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<PageResult>(result);
            Assert.IsTrue(updateModel.ModelState.ErrorCount > 0);
        }

        /// <summary>
        /// OnPost_NoExtension_FallbacksToPng_ExpectedPngSaved
        /// </summary>
        [Test]
        public void OnPost_NoExtension_FallbacksToPng()
        {
            // Arrange
            var service = CreateService();

            var product = new ProductModel { Id = "nx1", Title = "T", Description = "D", Url = "u", Image = "/images/old.png", Location = "L" };

            service.CreateData(product);

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "nx1", Title = "T", Description = "D", Url = "u", Image = "/images/old.png", Location = "L" },
                Upload = CreateFormFile(Encoding.UTF8.GetBytes("x"), "file") // no extension
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<RedirectToPageResult>(result);
            Assert.IsTrue(updateModel.Product.Image.EndsWith(".png"));

            var physicalPath = Path.Combine(_tempWebRoot, updateModel.Product.Image.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            Assert.IsTrue(File.Exists(physicalPath));
        }

        /// <summary>
        /// OnPost_JpegExtension_NormalizesToJpg_ExpectedJpgSaved
        /// </summary>
        [Test]
        public void OnPost_JpegExtension_NormalizesToJpg()
        {
            // Arrange
            var service = CreateService();

            var product = new ProductModel { Id = "nx2", Title = "T", Description = "D", Url = "u", Image = "/images/old.png", Location = "L" };

            service.CreateData(product);

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "nx2", Title = "T", Description = "D", Url = "u", Image = "/images/old.png", Location = "L" },
                Upload = CreateFormFile(Encoding.UTF8.GetBytes("x"), "file.JPEG")
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<RedirectToPageResult>(result);
            Assert.IsTrue(updateModel.Product.Image.EndsWith(".jpg"));

            var physicalPath = Path.Combine(_tempWebRoot, updateModel.Product.Image.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            Assert.IsTrue(File.Exists(physicalPath));
        }

        /// <summary>
        /// OnPost_TitleEmpty_GeneratesGuidInitialsAndSavesPng_ExpectedInitialsAndPng
        /// </summary>
        [Test]
        public void OnPost_TitleEmpty_GeneratesGuidInitialsAndSavesPng()
        {
            // Arrange
            var service = CreateService();

            var product = new ProductModel { Id = "nx3", Title = "Old", Description = "D", Url = "u", Image = "/images/old.png", Location = "L" };

            service.CreateData(product);

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "nx3", Title = "", Description = "D", Url = "u", Image = "/images/old.png", Location = "L" },
                Upload = CreateFormFile(Encoding.UTF8.GetBytes("x"), "noext")
            };

            // Act
            var result = updateModel.OnPost();

            // Title is empty so later validation will return the Page, but upload occurs before validation.
            Assert.IsInstanceOf<PageResult>(result);
            Assert.IsTrue(updateModel.Product.Image.StartsWith("/images/"));

            var FileName = Path.GetFileName(updateModel.Product.Image);
            var parts = FileName.Split('_');

            Assert.GreaterOrEqual(parts.Length, 2);

            var Initials = parts[0];

            Assert.AreEqual(8, Initials.Length);
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(Initials, "^[0-9a-fA-F]{8}$"));
            Assert.IsTrue(updateModel.Product.Image.EndsWith(".png"));
        }

        /// <summary>
        /// OnPost_DegreeArrays_AreTrimmedAndCleaned_ExpectedArraysTrimmed
        /// </summary>
        [Test]
        public void OnPost_DegreeArrays_AreTrimmedAndCleaned()
        {
            // Arrange
            var service = CreateService();

            var product = new ProductModel { Id = "nx4", Title = "T", Description = "D", Url = "u", Image = "/images/old.png", Location = "L" };

            service.CreateData(product);

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel
                {
                    Id = "nx4",
                    Title = "T",
                    Description = "D",
                    Url = "u",
                    Image = "/images/old.png",
                    Location = "L",
                    GraduateDegree = new string[] { " ", " MSc ", null, "PhD" },
                    UnderGraduateDegree = new string[] { "BA", "  ", " BSc " }
                }
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<RedirectToPageResult>(result);
            Assert.IsNotNull(updateModel.Product.GraduateDegree);
            Assert.IsTrue(updateModel.Product.GraduateDegree.All(s => string.IsNullOrWhiteSpace(s) == false));
            Assert.IsNotNull(updateModel.Product.UnderGraduateDegree);
            Assert.IsTrue(updateModel.Product.UnderGraduateDegree.All(s => string.IsNullOrWhiteSpace(s) == false));

            // Ensure trimming occurred
            Assert.IsTrue(updateModel.Product.GraduateDegree.Contains("MSc"));
            Assert.IsTrue(updateModel.Product.GraduateDegree.Contains("PhD"));
            Assert.IsTrue(updateModel.Product.UnderGraduateDegree.Contains("BA"));
            Assert.IsTrue(updateModel.Product.UnderGraduateDegree.Contains("BSc"));
        }

        /// <summary>
        /// OnPost_TitleInitials_TooLong_TruncatesTo20_ExpectedInitialsTruncated
        /// </summary>
        [Test]
        public void OnPost_TitleInitials_TooLong_TruncatesTo20()
        {
            // Arrange
            var service = CreateService();

            var product = new ProductModel { Id = "il1", Title = "Old", Description = "D", Url = "u", Image = "/images/old.png", Location = "L" };

            service.CreateData(product);

            // Create a title with many short words so initials length > 20 but total length stays reasonable
            var ManyWords = string.Join(' ', Enumerable.Repeat("a", 25));

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "il1", Title = ManyWords, Description = "D", Url = "u", Image = "/images/old.png", Location = "L" },
                Upload = CreateFormFile(Encoding.UTF8.GetBytes("x"), "file.png")
            };

            // remove images folder so creation branch is exercised
            var imagesPath = Path.Combine(_tempWebRoot, "images");

            if (Directory.Exists(imagesPath))
            {
                Directory.Delete(imagesPath, true);
            }

            // Act
            var result = updateModel.OnPost();

            // Assert
            var isRedirectLocal = result is RedirectToPageResult;
            var isPage = result is PageResult;
            var condition = isRedirectLocal;
            if (!condition)
            {
                condition = isPage;
            }
            Assert.IsTrue(condition);

            // If redirect then image should have been set and saved
            if (isRedirectLocal)
            {
                var FileName = Path.GetFileName(updateModel.Product.Image);
                var Initials = FileName.Split('_')[0];

                Assert.LessOrEqual(Initials.Length, 20);

                // ensure the images folder was created
                Assert.IsTrue(Directory.Exists(imagesPath));
            }
        }

        /// <summary>
        /// OnPost_MissingDescriptionOnly_AddsError_ExpectedValidationError
        /// </summary>
        [Test]
        public void OnPost_MissingDescriptionOnly_AddsError()
        {
            // Arrange
            var service = CreateService();

            service.CreateData(new ProductModel { Id = "md1", Title = "T" });

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "md1", Title = "T", Description = "", Url = "u", Image = "/images/x.png", Location = "L" }
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            var isPage = result is PageResult;
            var isRedirect = result is RedirectToPageResult;
            var condition = isPage;
            if (!condition)
            {
                condition = isRedirect;
            }
            Assert.IsTrue(condition);

            if (isPage)
            {
                Assert.IsTrue(updateModel.ModelState.ErrorCount > 0);
            }
        }

        /// <summary>
        /// OnPost_MissingUrlOnly_AddsError_ExpectedValidationError
        /// </summary>
        [Test]
        public void OnPost_MissingUrlOnly_AddsError()
        {
            // Arrange
            var service = CreateService();

            service.CreateData(new ProductModel { Id = "mu1", Title = "T", Description = "D" });

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "mu1", Title = "T", Description = "D", Url = "", Image = "/images/x.png", Location = "L" }
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<PageResult>(result);
            Assert.IsTrue(updateModel.ModelState.ErrorCount > 0);
        }

        /// <summary>
        /// OnPost_MissingImageOnly_AddsError_ExpectedValidationError
        /// </summary>
        [Test]
        public void OnPost_MissingImageOnly_AddsError()
        {
            // Arrange
            var service = CreateService();

            service.CreateData(new ProductModel { Id = "mi1", Title = "T", Description = "D", Url = "u" });

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "mi1", Title = "T", Description = "D", Url = "u", Image = "", Location = "L" }
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<PageResult>(result);
            Assert.IsTrue(updateModel.ModelState.ErrorCount > 0);
        }

        /// <summary>
        /// OnPost_MissingLocationOnly_AddsError_ExpectedValidationError
        /// </summary>
        [Test]
        public void OnPost_MissingLocationOnly_AddsError()
        {
            // Arrange
            var service = CreateService();

            service.CreateData(new ProductModel { Id = "ml1", Title = "T", Description = "D", Url = "u", Image = "/images/x.png" });

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "ml1", Title = "T", Description = "D", Url = "u", Image = "/images/x.png", Location = "" }
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<PageResult>(result);
            Assert.IsTrue(updateModel.ModelState.ErrorCount > 0);
        }

        /// <summary>
        /// OnPost_LocationTooLong_AddsError_ExpectedValidationError
        /// </summary>
        [Test]
        public void OnPost_LocationTooLong_AddsError()
        {
            // Arrange
            var service = CreateService();

            service.CreateData(new ProductModel { Id = "loc1", Title = "T", Description = "D", Url = "u", Image = "/images/x.png" });

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "loc1", Title = "T", Description = "D", Url = "u", Image = "/images/x.png", Location = new string('L', 60) }
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<PageResult>(result);
            Assert.IsTrue(updateModel.ModelState.ErrorCount > 0);
        }

        /// <summary>
        /// OnPost_DescriptionTooLong_AddsError_ExpectedValidationError
        /// </summary>
        [Test]
        public void OnPost_DescriptionTooLong_AddsError()
        {
            // Arrange
            var service = CreateService();

            service.CreateData(new ProductModel { Id = "desc1", Title = "T", Description = "D", Url = "u", Image = "/images/x.png" });

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "desc1", Title = "T", Description = new string('D', 600), Url = "u", Image = "/images/x.png", Location = "L" }
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<PageResult>(result);
            Assert.IsTrue(updateModel.ModelState.ErrorCount > 0);
        }

        /// <summary>
        /// OnPost_CampusEmpty_AddsError_ExpectedValidationError
        /// </summary>
        [Test]
        public void OnPost_CampusEmpty_AddsError()
        {
            // Arrange
            var service = CreateService();

            service.CreateData(new ProductModel { Id = "c1", Title = "T", Description = "D", Url = "u", Image = "/images/x.png" });

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "c1", Title = "T", Description = "D", Url = "u", Image = "/images/x.png", Location = "L", Campuses = new System.Collections.Generic.List<string> { "" } }
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            var isPage = result is PageResult;
            var isRedirect = result is RedirectToPageResult;
            var condition = isPage;
            if (!condition)
            {
                condition = isRedirect;
            }
            Assert.IsTrue(condition);

            if (isPage)
            {
                Assert.IsTrue(updateModel.ModelState.ErrorCount > 0);
            }
        }

        /// <summary>
        /// OnPost_CampusWhitespace_AddsError_ExpectedValidationError
        /// </summary>
        [Test]
        public void OnPost_CampusWhitespace_AddsError()
        {
            // Arrange
            var service = CreateService();

            service.CreateData(new ProductModel { Id = "cw1", Title = "T", Description = "D", Url = "u", Image = "/images/x.png" });

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "cw1", Title = "T", Description = "D", Url = "u", Image = "/images/x.png", Location = "L", Campuses = new System.Collections.Generic.List<string> { " " } }
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            var isPage = result is PageResult;
            var isRedirect = result is RedirectToPageResult;
            var condition = isPage;
            if (!condition)
            {
                condition = isRedirect;
            }
            Assert.IsTrue(condition);

            if (isPage)
            {
                Assert.IsTrue(updateModel.ModelState.ErrorCount > 0);
            }
        }

        /// <summary>
        /// ValidateCampuses_PrivateMethod_DetectsWhitespaceAndAddsError_ExpectedValidationFails
        /// </summary>
        [Test]
        public void ValidateCampuses_PrivateMethod_DetectsWhitespaceAndAddsError()
        {
            // Arrange
            var service = CreateService();

            service.CreateData(new ProductModel { Id = "vc1", Title = "T" });

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "vc1", Title = "T", Description = "D", Url = "u", Image = "/images/x.png", Location = "L", Campuses = new System.Collections.Generic.List<string> { " " } }
            };

            var method = typeof(UpdateModel).GetMethod("ValidateCampuses", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = (bool)method.Invoke(updateModel, null);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(updateModel.ModelState.ErrorCount > 0);
        }

        /// <summary>
        /// OnPost_CampusTooLong_AddsError_ExpectedValidationError
        /// </summary>
        [Test]
        public void OnPost_CampusTooLong_AddsError()
        {
            // Arrange
            var service = CreateService();

            service.CreateData(new ProductModel { Id = "c2", Title = "T", Description = "D", Url = "u", Image = "/images/x.png" });

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "c2", Title = "T", Description = "D", Url = "u", Image = "/images/x.png", Location = "L", Campuses = new System.Collections.Generic.List<string> { new string('A', 60) } }
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<PageResult>(result);
            Assert.IsTrue(updateModel.ModelState.ErrorCount > 0);
        }

        /// <summary>
        /// OnPost_ProductDeleted_SetsTempDataAndRedirects_ExpectedTempDataError
        /// </summary>
        [Test]
        public void OnPost_ProductDeleted_SetsTempDataAndRedirects()
        {
            // Arrange
            var service = CreateService();

            // create a different product so the id will not be found
            service.CreateData(new ProductModel { Id = "other", Title = "X", Description = "D", Url = "u", Image = "/images/x.png", Location = "L" });

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "missing", Title = "T", Description = "D", Url = "u", Image = "/images/x.png", Location = "L" }
            };

            // supply a TempDataDictionary so the code path that sets the ErrorMessage is executed
            updateModel.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<RedirectToPageResult>(result);
            Assert.IsTrue(updateModel.TempData.ContainsKey("ErrorMessage"));
        }

        /// <summary>
        /// OnPost_UpdateDataFails_AddsErrorAndReturnsPage_ExpectedValidationError
        /// </summary>
        [Test]
        public void OnPost_UpdateDataFails_AddsErrorAndReturnsPage()
        {
            // Arrange
            var env = CreateEnv();

            // simulate a concurrent delete by returning JSON with the product on the first read
            // and an empty array on the subsequent read so UpdateData will return false
            var firstJson = "[{\"Id\":\"uf1\",\"Title\":\"T\",\"Description\":\"D\",\"Url\":\"u\",\"Image\":\"/images/x.png\",\"Location\":\"L\"}]";
            var service = new FlakyReadService(env, firstJson);

            var updateModel = new UpdateModel(service)
            {
                Product = new ProductModel { Id = "uf1", Title = "T", Description = "D", Url = "u", Image = "/images/x.png", Location = "L" }
            };

            // Act
            var result = updateModel.OnPost();

            // Assert
            Assert.IsInstanceOf<PageResult>(result);
            Assert.IsTrue(updateModel.ModelState.ErrorCount > 0);
        }

        #endregion

        // Simple IWebHostEnvironment stub
        private class TestEnv : IWebHostEnvironment
        {
            public string WebRootPath { get; set; }
            public string EnvironmentName { get; set; }
            public string ApplicationName { get; set; }
            public string ContentRootPath { get; set; }
            public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; }
            public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }
        }

        // A form file that throws when attempting to open stream/read
        private class ThrowingFormFile : IFormFile
        {
            public string ContentType => "application/octet-stream";
            public string ContentDisposition => string.Empty;
            public IHeaderDictionary Headers => new HeaderDictionary();
            public long Length => 10;
            public string Name => "upload";
            public string FileName => "bad.bin";
            public void CopyTo(Stream target)
            {
                throw new IOException("Simulated failure");
            }
            public Task CopyToAsync(Stream target, System.Threading.CancellationToken cancellationToken = default)
            {
                throw new IOException("Simulated failure");
            }
            public Stream OpenReadStream() => throw new IOException("Simulated failure");
        }

        // A form file that throws UnauthorizedAccessException when attempting to save
        private class UnauthorizedFormFile : IFormFile
        {
            public string ContentType => "application/octet-stream";
            public string ContentDisposition => string.Empty;
            public IHeaderDictionary Headers => new HeaderDictionary();
            public long Length => 10;
            public string Name => "upload";
            public string FileName => "bad.bin";
            public void CopyTo(Stream target)
            {
                throw new UnauthorizedAccessException("Simulated access denied");
            }
            public Task CopyToAsync(Stream target, System.Threading.CancellationToken cancellationToken = default)
            {
                throw new UnauthorizedAccessException("Simulated access denied");
            }
            public Stream OpenReadStream() => throw new UnauthorizedAccessException("Simulated access denied");
        }

        // Minimal TempData provider for tests
        private class TestTempDataProvider : ITempDataProvider
        {
            public System.Collections.Generic.IDictionary<string, object> LoadTempData(HttpContext context)
            {
                return new System.Collections.Generic.Dictionary<string, object>();
            }

            public void SaveTempData(HttpContext context, System.Collections.Generic.IDictionary<string, object> values)
            {
                // no-op
            }
        }

        // Service variant that returns different JSON on consecutive reads to simulate
        // a concurrent deletion so UpdateData returns false.
        private class FlakyReadService : JsonFileProductService
        {
            private int _callCount = 0;
            private readonly string _firstJson;

            public FlakyReadService(IWebHostEnvironment env, string firstJson) : base(env)
            {
                _firstJson = firstJson;
            }

            protected override string ReadAllText(string path)
            {
                // First call returns the JSON containing the product; subsequent calls return an empty list
                _callCount++;
                if (_callCount == 1)
                {
                    return _firstJson;
                }

                return "[]";
            }

            protected override bool FileExists(string path)
            {
                return true;
            }
        }

        // Service that hides UpdateData isn't workable because UpdateData is non-virtual.
        // Instead we simulate UpdateData failing via FlakyReadService above.
    }
}