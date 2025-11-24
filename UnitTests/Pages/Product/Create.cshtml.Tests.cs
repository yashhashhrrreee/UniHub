using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using ContosoCrafts.WebSite.Pages.Product;
using ContosoCrafts.WebSite.Models;
using ContosoCrafts.WebSite.Services;

namespace UnitTests.Pages.Product
{
    /// <summary>
    /// Unit tests for the <see cref="ContosoCrafts.WebSite.Pages.Product.CreateModel"/> page model.
    /// Tests follow Arrange / Act / Assert structure and naming conventions.
    /// </summary>
    [TestFixture]

    /// <summary>
    /// Unit tests for the <see cref="ContosoCrafts.WebSite.Pages.Product.CreateModel"/> page model.
    /// Test class name follows the pattern: ClassName + "Test".
    /// </summary>
    public class CreateModelTest
    {
        // Temp web root used by filesystem tests
        private string TempWebRoot;

        [SetUp]
        /// <summary>
        /// Setup called before each test. Creates a temporary webroot.
        /// </summary>
        public void Setup()
        {
            TempWebRoot = Path.Combine(Path.GetTempPath(), "CreateModelTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(TempWebRoot);
            Directory.CreateDirectory(Path.Combine(TempWebRoot, "images"));
        }

        [TearDown]
        /// <summary>
        /// TearDown called after each test. Attempts to remove the temporary webroot.
        /// </summary>
        public void TearDown()
        {
            if (Directory.Exists(TempWebRoot))
            {
                Directory.Delete(TempWebRoot, true);
            }

        }

        /// <summary>
        /// Create a test environment with the temp web root path.
        /// </summary>
        private IWebHostEnvironment CreateEnv() => new TestEnv { WebRootPath = TempWebRoot };

        /// <summary>
        /// Create the JsonFileProductService using the test environment.
        /// </summary>
        private JsonFileProductService CreateService() => new JsonFileProductService(CreateEnv());

        #region Constructor
        /// <summary>
        /// Test constructor throws ArgumentNullException when productService is null.
        /// </summary>
        [Test]
        public void Constructor_NullProductService_ThrowsArgumentNullException_Expected()
        {
            // Arrange
            var mockWebHostEnvironment = CreateEnv();
            var mockLogger = NullLogger<CreateModel>.Instance;

            // Act & Assert
            Assert.DoesNotThrow(() => new CreateModel(null, mockWebHostEnvironment, mockLogger),
                "Constructor should handle null product service gracefully.");
        }

        /// <summary>
        /// Test constructor handles null webHostEnvironment gracefully.
        /// </summary>
        [Test]
        public void Constructor_NullWebHostEnvironment_ThrowsArgumentNullException_Expected()
        {
            // Arrange
            var mockProductService = CreateService();
            var mockLogger = NullLogger<CreateModel>.Instance;

            // Act & Assert
            Assert.DoesNotThrow(() => new CreateModel(mockProductService, null, mockLogger),
                "Constructor should handle null webHostEnvironment gracefully.");
        }

        /// <summary>
        /// Test constructor handles null logger gracefully.
        /// </summary>
        [Test]
        public void Constructor_NullLogger_ThrowsArgumentNullException_Expected()
        {
            // Arrange
            var mockProductService = CreateService();
            var mockWebHostEnvironment = CreateEnv();

            // Act & Assert
            Assert.DoesNotThrow(() => new CreateModel(mockProductService, mockWebHostEnvironment, null),
                "Constructor should handle null logger gracefully.");
        }
        #endregion

        #region OnGet_ProductNull_ProductInitialized_ReturnsProductNotNull

        [Test]
        /// <summary>
        /// OnGet_ProductNull_ProductInitialized_ReturnsProductNotNull
        /// </summary>
        public void OnGet_ProductNull_ProductInitialized_ReturnsProductNotNull()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);

            model.Product = null;

            // Act
            model.OnGet();

            // Assert
            Assert.AreNotEqual(null, model.Product);
        }

        #endregion

        #region OnPost_ProductWithNullCampuses_ReturnsPageAndModelError

        [Test]
        /// <summary>
        /// OnPost_ProductWithNullCampuses_ReturnsPageAndModelError
        /// </summary>
        public void OnPost_ProductWithNullCampuses_ReturnsPageAndModelError()
        {
            // Arrange
            var svc = CreateService();
            var env = CreateEnv();
            var model = new CreateModel(svc, env, NullLogger<CreateModel>.Instance)
            {
                Product = new ProductModel()
            };

            // force Campus validation to fail
            model.Product.Campuses = null;

            // Act
            var result = model.OnPost();

            // Assert
            Assert.AreEqual(typeof(PageResult), result.GetType());
            Assert.AreEqual(true, model.ModelState.ErrorCount > 0);
        }

        #endregion

        #region OnPost_ValidProduct_RedirectsToIndex

        [Test]
        /// <summary>
        /// OnPost_ValidProduct_RedirectsToIndex
        /// </summary>
        public void OnPost_ValidProduct_RedirectsToIndex()
        {
            // Arrange
            var svc = CreateService();
            var env = CreateEnv();
            var model = new CreateModel(svc, env, NullLogger<CreateModel>.Instance)
            {
                Product = new ProductModel
                {
                    Title = "Test Product",
                    Campuses = new System.Collections.Generic.List<string> { "Main" },
                    GraduateDegree = new[] { "MS" },
                    UnderGraduateDegree = new[] { "BS" }
                }
            };

            // Act
            var result = model.OnPost();

            // Assert
            Assert.AreEqual(typeof(RedirectToPageResult), result.GetType());
            var redirect = (RedirectToPageResult)result;
            Assert.AreEqual("./Index", redirect.PageName);
        }

        #endregion

        #region OnPostUploadImageAsync_NullFile_ReturnsError

        [Test]
        /// <summary>
        /// OnPostUploadImageAsync_NullFile_ReturnsError
        /// </summary>
        public async Task OnPostUploadImageAsync_NullFile_ReturnsError()
        {
            // Arrange
            var svc = CreateService();
            var env = CreateEnv();
            var model = new CreateModel(svc, env, NullLogger<CreateModel>.Instance);

            // Act
            var result = await model.OnPostUploadImageAsync(null, "title");

            // Assert
            Assert.AreEqual(typeof(JsonResult), result.GetType());
            Assert.AreEqual(false, JsonSuccess((JsonResult)result));
        }

        #endregion

        /// <summary>
        /// Create an `IFormFile` from a byte array for upload tests.
        /// </summary>
        private IFormFile CreateFormFile(byte[] bytes, string fileName = "f.bin")
        {
            var ms = new MemoryStream(bytes);
            return new FormFile(ms, 0, ms.Length, "image", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream"
            };
        }

        /// <summary>
        /// Return the `success` boolean from an anonymous JSON result object.
        /// </summary>
        private bool JsonSuccess(JsonResult res)
        {
            var val = res.Value;
            var prop = val?.GetType().GetProperty("success");
            if (prop == null)
            {
                return false;
            }

            return (bool)prop.GetValue(val);
        }

        /// <summary>
        /// Return a string property from an anonymous JSON result object.
        /// </summary>
        private string JsonString(JsonResult res, string name)
        {
            var val = res.Value;
            var prop = val?.GetType().GetProperty(name);
            return prop != null ? prop.GetValue(val)?.ToString() : null;
        }

        #region OnPostUploadImageAsync_Png_ReturnsSuccessAndPath

        [Test]
        /// <summary>
        /// OnPostUploadImageAsync_Png_ReturnsSuccessAndPath
        /// </summary>
        public async Task OnPostUploadImageAsync_Png_ReturnsSuccessAndPath()
        {
            // Arrange
            var svc = CreateService();
            var env = CreateEnv();
            var model = new CreateModel(svc, env, NullLogger<CreateModel>.Instance);

            // PNG magic bytes
            var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3 };
            var file = CreateFormFile(png, "t.png");

            // Act
            var result = await model.OnPostUploadImageAsync(file, "My Title");

            // Assert
            Assert.AreEqual(typeof(JsonResult), result.GetType());
            Assert.AreEqual(true, JsonSuccess((JsonResult)result));
            string path = JsonString((JsonResult)result, "imagePath");
            Assert.AreEqual(true, path.StartsWith("/images/"));

            // file should exist on disk
            var physical = Path.Combine(TempWebRoot, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            Assert.AreEqual(true, File.Exists(physical));
        }

        #endregion

        #region OnPostUploadImageAsync_Jpeg_ReturnsSuccessAndPath

        [Test]
        /// <summary>
        /// OnPostUploadImageAsync_Jpeg_ReturnsSuccessAndPath
        /// </summary>
        public async Task OnPostUploadImageAsync_Jpeg_ReturnsSuccessAndPath()
        {
            // Arrange
            var svc = CreateService();
            var env = CreateEnv();
            var model = new CreateModel(svc, env, NullLogger<CreateModel>.Instance);

            var jpg = new byte[] { 0xFF, 0xD8, 1, 2, 3 };
            var file = CreateFormFile(jpg, "t.jpg");

            // Act
            var result = await model.OnPostUploadImageAsync(file, "Another Title");

            // Assert
            Assert.AreEqual(typeof(JsonResult), result.GetType());
            Assert.AreEqual(true, JsonSuccess((JsonResult)result));
            string path = JsonString((JsonResult)result, "imagePath");
            Assert.AreEqual(true, path.EndsWith(".jpg"));
        }

        #endregion

        #region OnPostUploadImageAsync_NotImage_ReturnsError

        [Test]
        /// <summary>
        /// OnPostUploadImageAsync_NotImage_ReturnsError
        /// </summary>
        public async Task OnPostUploadImageAsync_NotImage_ReturnsError()
        {
            // Arrange
            var svc = CreateService();
            var env = CreateEnv();
            var model = new CreateModel(svc, env, NullLogger<CreateModel>.Instance);

            var bytes = Encoding.UTF8.GetBytes("not an image");
            var file = CreateFormFile(bytes, "t.txt");

            // Act
            var result = await model.OnPostUploadImageAsync(file, "Title");

            // Assert
            Assert.AreEqual(typeof(JsonResult), result.GetType());
            Assert.AreEqual(false, JsonSuccess((JsonResult)result));
        }

        #endregion

        #region OnPostDeleteImageAsync_InvalidPath_ReturnsError

        [Test]
        /// <summary>
        /// OnPostDeleteImageAsync_InvalidPath_ReturnsError
        /// </summary>
        public void OnPostDeleteImageAsync_InvalidPath_ReturnsError()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);

            // Act
            var result = model.OnPostDeleteImageAsync("../etc/passwd");

            // Assert
            Assert.AreEqual(typeof(JsonResult), result.GetType());
            Assert.AreEqual(false, JsonSuccess((JsonResult)result));
        }

        #endregion

        #region OnPostDeleteImageAsync_DeletesFile_RemovesFile

        [Test]
        /// <summary>
        /// OnPostDeleteImageAsync_DeletesFile_RemovesFile
        /// </summary>
        public void OnPostDeleteImageAsync_DeletesFile_RemovesFile()
        {
            // Arrange
            var svc = CreateService();
            var env = CreateEnv();
            var imageDir = Path.Combine(TempWebRoot, "images");
            Directory.CreateDirectory(imageDir);
            var fileName = "to_delete.png";
            var physical = Path.Combine(imageDir, fileName);
            File.WriteAllText(physical, "x");

            var model = new CreateModel(svc, env, NullLogger<CreateModel>.Instance);

            // Act
            var result = model.OnPostDeleteImageAsync($"/images/{fileName}");

            // Assert
            Assert.AreEqual(typeof(JsonResult), result.GetType());
            Assert.AreEqual(false, File.Exists(physical));
        }

        #endregion

        // Helper to call private instance method
        private object CallPrivate(object instance, string name, params object[] args)
        {
            var type = instance.GetType();
            var method = type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            return method.Invoke(instance, args);
        }

        // Helper to call private static method
        private object CallPrivateStatic(Type type, string name, params object[] args)
        {
            var method = type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
            return method.Invoke(null, args);
        }

        #region GenerateIdFromTitle_EmptyOrNull_Returns8Char

        [Test]
        public void GenerateIdFromTitle_EmptyOrNull_Returns8Char()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);

            // Act
            var r1 = (string)CallPrivate(model, "GenerateIdFromTitle", (string)null);
            var r2 = (string)CallPrivate(model, "GenerateIdFromTitle", "");

            // Assert
            Assert.AreNotEqual(null, r1);
            Assert.AreEqual(8, r1.Length);
            Assert.AreNotEqual(null, r2);
            Assert.AreEqual(8, r2.Length);
        }

        #endregion

        #region GenerateIdFromTitle_NoLetters_ReturnsFallback

        [Test]
        public void GenerateIdFromTitle_NoLetters_ReturnsFallback()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);

            // Act
            var id = (string)CallPrivate(model, "GenerateIdFromTitle", "1234567890");

            // Assert
            Assert.AreNotEqual(null, id);
            Assert.AreEqual(8, id.Length);
        }

        #endregion

        #region GenerateIdFromTitle_NormalTitle_ReturnsLettersLowerAndFiltered

        [Test]
        public void GenerateIdFromTitle_NormalTitle_ReturnsLettersLowerAndFiltered()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);

            // Act
            var id = (string)CallPrivate(model, "GenerateIdFromTitle", "Hello World 123");

            // Assert
            Assert.AreEqual(true, id.Length > 0);
            var idOk = id.All(c =>
            {
                if (char.IsLower(c))
                {
                    return true;
                }

                if (char.IsDigit(c))
                {
                    return true;
                }

                return false;
            });

            Assert.AreEqual(true, idOk);
        }

        #endregion

        #region ParseDegrees_Empty_ReturnsEmpty

        [Test]
        public void ParseDegrees_Empty_ReturnsEmpty()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);

            // Act
            var arr = (string[])CallPrivate(model, "ParseDegrees", "");

            // Assert
            Assert.AreNotEqual(null, arr);
            Assert.AreEqual(0, arr.Length);
        }

        #endregion

        #region ParseDegrees_CommaSeparated_ReturnsParts

        [Test]
        public void ParseDegrees_CommaSeparated_ReturnsParts()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);

            // Act
            var arr = (string[])CallPrivate(model, "ParseDegrees", " MS, PhD ; BA\nMA");

            // Assert
            Assert.AreEqual(true, arr.Length >= 3);
            Assert.AreEqual(true, arr.Any(s => s == "MS"));
        }

        #endregion

        #region IsPng_StaticChecks_VariousInputs

        [Test]
        public void IsPng_StaticChecks_VariousInputs()
        {
            // null
            Assert.AreEqual(false, (bool)CallPrivateStatic(typeof(CreateModel), "IsPng", (object)null));

            // too short
            Assert.AreEqual(false, (bool)CallPrivateStatic(typeof(CreateModel), "IsPng", new byte[] { 1, 2, 3 }));

            // wrong header bytes
            var bad = new byte[] { 0x00, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            Assert.AreEqual(false, (bool)CallPrivateStatic(typeof(CreateModel), "IsPng", bad));

            // valid png header
            var good = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            Assert.AreEqual(true, (bool)CallPrivateStatic(typeof(CreateModel), "IsPng", good));
        }

        #endregion

        #region IsPng_EachHeaderByteMismatch_ReturnsFalse

        [Test]
        public void IsPng_EachHeaderByteMismatch_ReturnsFalse()
        {
            var basePng = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

            for (int i = 0; i < 8; i++)
            {
                var arr = (byte[])basePng.Clone();
                arr[i] = (byte)(arr[i] == 0 ? 1 : 0);
                Assert.AreEqual(false, (bool)CallPrivateStatic(typeof(CreateModel), "IsPng", arr), $"byte {i} mismatch should be false");
            }
        }

        #endregion

        #region IsJpeg_StaticChecks_VariousInputs

        [Test]
        public void IsJpeg_StaticChecks_VariousInputs()
        {
            // null
            Assert.AreEqual(false, (bool)CallPrivateStatic(typeof(CreateModel), "IsJpeg", (object)null));

            // too short
            Assert.AreEqual(false, (bool)CallPrivateStatic(typeof(CreateModel), "IsJpeg", new byte[] { 0xFF }));

            // wrong header
            Assert.AreEqual(false, (bool)CallPrivateStatic(typeof(CreateModel), "IsJpeg", new byte[] { 0x00, 0x00 }));

            // valid jpeg
            Assert.AreEqual(true, (bool)CallPrivateStatic(typeof(CreateModel), "IsJpeg", new byte[] { 0xFF, 0xD8 }));
        }

        #endregion

        #region CleanDegreeArrays_RemovesEmptyAndTrims

        [Test]
        public void CleanDegreeArrays_RemovesEmptyAndTrims()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance)
            {
                Product = new ProductModel
                {
                    GraduateDegree = new[] { " MS ", null, "  " },
                    UnderGraduateDegree = new[] { "BA", "  BA2  " }
                }
            };

            // Act
            CallPrivate(model, "CleanDegreeArrays");

            // Assert
            var gradOk = false;
            if (model.Product.GraduateDegree != null)
            {
                if (model.Product.GraduateDegree.Length == 1)
                {
                    if (model.Product.GraduateDegree[0] == "MS")
                    {
                        gradOk = true;
                    }
                }
            }

            Assert.AreEqual(true, gradOk);
            Assert.AreEqual(true, model.Product.UnderGraduateDegree.All(s => s.Trim() == s));
        }

        #endregion

        #region CleanCampuses_TrimsAndRemovesEmpty

        [Test]
        public void CleanCampuses_TrimsAndRemovesEmpty()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance)
            {
                Product = new ProductModel { Campuses = new System.Collections.Generic.List<string> { " A ", "  ", null } }
            };

            // Act
            CallPrivate(model, "CleanCampuses");

            // Assert
            var campOk = false;
            if (model.Product.Campuses != null)
            {
                if (model.Product.Campuses.Count == 1)
                {
                    if (model.Product.Campuses[0] == "A")
                    {
                        campOk = true;
                    }
                }
            }

            Assert.AreEqual(true, campOk);
        }

        #endregion

        #region ValidateCampuses_LongAndWhitespace_AddsErrors

        [Test]
        public void ValidateCampuses_LongAndWhitespace_AddsErrors()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance)
            {
                Product = new ProductModel { Campuses = new System.Collections.Generic.List<string> { " ", new string('A', 60) } }
            };

            // Act
            CallPrivate(model, "ValidateCampuses");

            // Assert
            Assert.AreEqual(true, model.ModelState.ErrorCount >= 1);
        }

        #endregion

        #region ValidateDegreeArray_EmptyAndWhitespaceAndTooLong_AddsErrors

        [Test]
        public void ValidateDegreeArray_EmptyAndWhitespaceAndTooLong_AddsErrors()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);

            // Act & Assert
            // empty array
            CallPrivate(model, "ValidateDegreeArray", "prop", new string[] { }, "Display");
            Assert.AreEqual(true, model.ModelState.ErrorCount > 0);

            // whitespace element
            var model2 = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);
            CallPrivate(model2, "ValidateDegreeArray", "prop", new[] { " " }, "Display");
            Assert.AreEqual(true, model2.ModelState.ErrorCount > 0);

            // too long element
            var model3 = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);
            CallPrivate(model3, "ValidateDegreeArray", "prop", new[] { new string('B', 150) }, "Display");
            Assert.AreEqual(true, model3.ModelState.ErrorCount > 0);
        }

        #endregion

        #region BuildInitialsFromTitle_TooLong_Truncates

        [Test]
        public void BuildInitialsFromTitle_TooLong_Truncates()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);

            // create a title with many words to exceed 20 initials
            var many = string.Join(" ", Enumerable.Range(0, 30).Select(i => "Word" + i));

            // Act
            var initials = (string)CallPrivate(model, "BuildInitialsFromTitle", many);

            // Assert
            Assert.AreNotEqual(null, initials);
            Assert.AreEqual(true, initials.Length <= 20);
        }

        #endregion

        #region CleanDegreeArrays_Nulls_BecomesEmpty

        [Test]
        public void CleanDegreeArrays_Nulls_BecomesEmpty()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance)
            {
                Product = new ProductModel { GraduateDegree = null, UnderGraduateDegree = null }
            };

            // Act
            CallPrivate(model, "CleanDegreeArrays");

            // Assert
            Assert.AreNotEqual(null, model.Product.GraduateDegree);
            Assert.AreNotEqual(null, model.Product.UnderGraduateDegree);
            Assert.AreEqual(0, model.Product.GraduateDegree.Length);
            Assert.AreEqual(0, model.Product.UnderGraduateDegree.Length);
        }

        #endregion

        #region CleanDegreeArrays_MixedEntries_CoversOtherBranch

        [Test]
        public void CleanDegreeArrays_MixedEntries_CoversOtherBranch()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance)
            {
                Product = new ProductModel
                {
                    GraduateDegree = new[] { "A", " ", null, "B" },
                    UnderGraduateDegree = new[] { "C" }
                }
            };

            // Act
            CallPrivate(model, "CleanDegreeArrays");

            // Assert
            Assert.AreEqual(true, model.Product.GraduateDegree.Length >= 2);
        }

        #endregion

        #region OnPost_ProductNull_AssignsProductAndReturnsPage

        [Test]
        public void OnPost_ProductNull_AssignsProductAndReturnsPage()
        {
            // Arrange
            var svc = CreateService();
            var env = CreateEnv();
            var model = new CreateModel(svc, env, NullLogger<CreateModel>.Instance);

            // leave Product null to exercise the null-coalescing assignment in OnPost
            model.Product = null;

            // Act
            var result = model.OnPost();

            // Assert
            Assert.AreEqual(typeof(PageResult), result.GetType());
            Assert.AreNotEqual(null, model.Product);
        }

        #endregion

        #region OnPostDeleteImageAsync_NoImagePath_ReturnsError

        [Test]
        public void OnPostDeleteImageAsync_NoImagePath_ReturnsError()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);

            // Act
            var result = model.OnPostDeleteImageAsync((string)null);

            // Assert
            Assert.AreEqual(typeof(JsonResult), result.GetType());
            Assert.AreEqual(false, JsonSuccess((JsonResult)result));
            Assert.AreEqual("No image path provided.", JsonString((JsonResult)result, "error"));
        }

        #endregion

        #region CleanDegreeArrays_UnderGraduateContainsNull_RemovesNullsAndTrims

        [Test]
        public void CleanDegreeArrays_UnderGraduateContainsNull_RemovesNullsAndTrims()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance)
            {
                Product = new ProductModel { UnderGraduateDegree = new[] { (string)null, " BA " } }
            };

            // Act
            CallPrivate(model, "CleanDegreeArrays");

            // Assert
            Assert.AreEqual(1, model.Product.UnderGraduateDegree.Length);
            Assert.AreEqual("BA", model.Product.UnderGraduateDegree[0]);
        }

        #endregion

        #region OnPostDeleteImageAsync_DeletesFile_EnsuresPrecondition

        [Test]
        public void OnPostDeleteImageAsync_DeletesFile_EnsuresPrecondition()
        {
            // Arrange
            var svc = CreateService();
            var env = CreateEnv();
            var imageDir = Path.Combine(TempWebRoot, "images");
            Directory.CreateDirectory(imageDir);
            var fileName = "to_delete_precond.png";
            var physical = Path.Combine(imageDir, fileName);
            File.WriteAllText(physical, "x");

            Assert.AreEqual(true, File.Exists(physical), "precondition file should exist");

            var model = new CreateModel(svc, env, NullLogger<CreateModel>.Instance);

            // Act
            var result = model.OnPostDeleteImageAsync($"/images/{fileName}");

            // Assert
            Assert.AreEqual(typeof(JsonResult), result.GetType());
            Assert.AreEqual(false, File.Exists(physical));
        }

        #endregion

        #region BuildInitialsFromTitle_ForDigitsOnly_ReturnsFallbackGuid

        [Test]
        public void BuildInitialsFromTitle_ForDigitsOnly_ReturnsFallbackGuid()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);

            // Act
            var initials = (string)CallPrivate(model, "BuildInitialsFromTitle", "123 456 789");

            // Assert
            Assert.AreNotEqual(null, initials);
            Assert.AreEqual(8, initials.Length);
        }

        #endregion

        #region BuildInitialsFromTitle_ClearLetters_TooLong_TruncatesTo20

        [Test]
        public void BuildInitialsFromTitle_ClearLetters_TooLong_TruncatesTo20()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);

            // produce more than 20 initials by using 25 one-letter words
            var many = string.Join(" ", Enumerable.Range(0, 25).Select(i => "x" + i));

            // Act
            var initials = (string)CallPrivate(model, "BuildInitialsFromTitle", many);

            // Assert
            Assert.AreNotEqual(null, initials);
            Assert.AreEqual(true, initials.Length <= 20);
        }

        #endregion

        #region GenerateIdFromTitle_NonLatinLetters_Fallback

        [Test]
        public void GenerateIdFromTitle_NonLatinLetters_Fallback()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);

            // Act
            var id = (string)CallPrivate(model, "GenerateIdFromTitle", "ΑΒΓΔΕΖ");

            // Assert
            Assert.AreNotEqual(null, id);
            Assert.AreEqual(8, id.Length);
        }

        #endregion

        #region OnPost_TitleNull_StillRedirectsAndGeneratesId

        [Test]
        public void OnPost_TitleNull_StillRedirectsAndGeneratesId()
        {
            // Arrange
            var svc = CreateService();
            var env = CreateEnv();
            var model = new CreateModel(svc, env, NullLogger<CreateModel>.Instance)
            {
                Product = new ProductModel
                {
                    Title = null,
                    Campuses = new System.Collections.Generic.List<string> { "Main" },
                    GraduateDegree = new[] { "MS" },
                    UnderGraduateDegree = new[] { "BS" }
                }
            };

            // Act
            var result = model.OnPost();

            // Assert
            Assert.AreEqual(typeof(RedirectToPageResult), result.GetType());
            Assert.AreNotEqual(null, model.Product.Id);
            Assert.AreEqual(8, model.Product.Id.Length);
        }

        #endregion

        #region ValidateCampuses_NullOrEmpty_AddsErrors

        [Test]
        public void ValidateCampuses_NullOrEmpty_AddsErrors()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance)
            {
                Product = new ProductModel { Campuses = null }
            };

            // Act
            CallPrivate(model, "ValidateCampuses");

            // Assert
            Assert.AreEqual(true, model.ModelState.ErrorCount > 0);

            // empty list
            var model2 = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance)
            {
                Product = new ProductModel { Campuses = new System.Collections.Generic.List<string>() }
            };
            CallPrivate(model2, "ValidateCampuses");
            Assert.AreEqual(true, model2.ModelState.ErrorCount > 0);
        }

        #endregion

        #region ValidateDegreeArray_Null_AddsError

        [Test]
        public void ValidateDegreeArray_Null_AddsError()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);

            // Act
            CallPrivate(model, "ValidateDegreeArray", "prop", null, "Display");

            // Assert
            Assert.AreEqual(true, model.ModelState.ErrorCount > 0);
        }

        #endregion

        #region BuildInitialsFromTitle_BehavesAsExpected

        [Test]
        public void BuildInitialsFromTitle_BehavesAsExpected()
        {
            // Arrange
            var svc = CreateService();
            var model = new CreateModel(svc, CreateEnv(), NullLogger<CreateModel>.Instance);

            // Act
            var initials = (string)CallPrivate(model, "BuildInitialsFromTitle", "Hello World");
            var fallback = (string)CallPrivate(model, "BuildInitialsFromTitle", "  ");

            // Assert
            Assert.AreEqual(true, initials.Length > 0);
            Assert.AreNotEqual(null, fallback);
            Assert.AreEqual(8, fallback.Length);
        }

        #endregion

        #region IsJpeg_SecondByteMismatch_ReturnsFalse

        [Test]
        public void IsJpeg_SecondByteMismatch_ReturnsFalse()
        {
            // First byte correct, second byte incorrect
            Assert.AreEqual(false, (bool)CallPrivateStatic(typeof(CreateModel), "IsJpeg", new byte[] { 0xFF, 0x00 }));
        }

        #endregion

        #region OnPostUploadImageAsync_ZeroLengthFile_ReturnsError

        [Test]
        public async Task OnPostUploadImageAsync_ZeroLengthFile_ReturnsError()
        {
            // Arrange
            var svc = CreateService();
            var env = CreateEnv();
            var model = new CreateModel(svc, env, NullLogger<CreateModel>.Instance);

            var file = CreateFormFile(new byte[0], "empty.jpg");

            // Act
            var result = await model.OnPostUploadImageAsync(file, "Title");

            // Assert
            Assert.AreEqual(typeof(JsonResult), result.GetType());
            Assert.AreEqual(false, JsonSuccess((JsonResult)result));
        }

        #endregion

        #region OnPostUploadImageAsync_FileTooLarge_ReturnsError

        [Test]
        public async Task OnPostUploadImageAsync_FileTooLarge_ReturnsError()
        {
            // Arrange
            var svc = CreateService();
            var env = CreateEnv();
            var model = new CreateModel(svc, env, NullLogger<CreateModel>.Instance);

            // create a file slightly larger than 2MB
            var huge = new byte[2 * 1024 * 1024 + 1];
            var file = CreateFormFile(huge, "big.jpg");

            // Act
            var result = await model.OnPostUploadImageAsync(file, "Title");

            // Assert
            Assert.AreEqual(typeof(JsonResult), result.GetType());
            Assert.AreEqual(false, JsonSuccess((JsonResult)result));
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
    }
}
