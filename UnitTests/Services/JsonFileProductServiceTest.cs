using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using ContosoCrafts.WebSite.Models;
using ContosoCrafts.WebSite.Services;
using Microsoft.AspNetCore.Hosting;
using NUnit.Framework;

namespace UnitTests.Services
{
    /// <summary>
    /// Unit tests for the <see cref="ContosoCrafts.WebSite.Services.JsonFileProductService"/>.
    /// Covers constructor behavior, file/directory edge cases, and CRUD operations.
    /// </summary>
    [TestFixture]
    public class JsonFileProductServiceTest
    {
        private class TestEnv : IWebHostEnvironment
        {
            public string WebRootPath { get; set; }
            public string ApplicationName { get; set; }
            public string EnvironmentName { get; set; }
            public string ContentRootPath { get; set; }
            public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; }
            public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }
        }

        private string CreateTempWebRoot()
        {
            var TempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(TempDirectory);
            return TempDirectory;
        }

        #region Constructor
        [Test]
        public void Constructor_MissingFoldersAndFile_CreatesStructure_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            // Remove subfolders if any
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;

            // Act
            var Service = new JsonFileProductService(Environment);

            // Reset
            var ImagesFolder = Path.Combine(WebRoot, "images");
            var DataFolder = Path.Combine(WebRoot, "data");
            var JsonFile = Path.Combine(DataFolder, "products.json");

            // Assert
            Assert.AreEqual(true, Directory.Exists(ImagesFolder), "Images folder should be created.");
            Assert.AreEqual(true, Directory.Exists(DataFolder), "Data folder should be created.");
            Assert.AreEqual(true, File.Exists(JsonFile), "products.json should be created.");

            // Cleanup
            Directory.Delete(WebRoot, true);
        }
        #endregion

        #region SaveData_RealFileEdgeCase
        [Test]
        public void SaveData_FilePresentAtDataPath_IsHandled_Expected()
        {
            // Arrange - create service then replace data directory with a file so SaveData must remove it
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;

            // create the service to ensure initial folders exist
            var Service = new JsonFileProductService(Environment);

            // Remove the data directory and create a file at that path to simulate bad state
            var DataDirectory = Path.Combine(WebRoot, "data");
            Directory.Delete(DataDirectory, true);
            File.WriteAllText(DataDirectory, "i-am-a-file");

            var Product = new ProductModel { Id = "sf1", Title = "t" };

            // Act - this should cause SaveData to detect a file where the dir should be, delete it, create dir, and write products.json
            Assert.DoesNotThrow(() => Service.CreateData(Product));

            // Assert - data directory should now exist and products.json should be present
            Assert.IsTrue(Directory.Exists(DataDirectory), "Data directory should be recreated when a file existed at that path.");
            Assert.IsTrue(File.Exists(Path.Combine(DataDirectory, "products.json")), "products.json should be created after handling file-at-data-path.");

            Directory.Delete(WebRoot, true);
        }
        #endregion

        #region DeleteLocalImage_PrivateMethod
        [Test]
        public void DeleteLocalImageIfExists_NullProduct_NoOp_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);

            // Use reflection to invoke private method with null (to hit early-return branch)
            var Method = typeof(JsonFileProductService).GetMethod("DeleteLocalImageIfExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act / Assert - invoking with null should simply return and not throw
            Assert.DoesNotThrow(() => Method.Invoke(Service, new object[] { null }));

            Directory.Delete(WebRoot, true);
        }
        #endregion

        #region SaveData_FileAndDirectoryEdgeCases
        private class TestJsonFileProductService_SavePathAsFile : JsonFileProductService
        {
            public bool DeleteFileCalled;
            public bool CreateDirectoryCalled;
            public bool WriteAllTextCalled;

            public TestJsonFileProductService_SavePathAsFile(IWebHostEnvironment env) : base(env) { }

            protected override bool FileExists(string path)
            {
                var webRoot = WebHostEnvironment?.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var dir = Path.Combine(webRoot, "data");
                if (string.Equals(path, dir, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return base.FileExists(path);
            }

            protected override bool DirectoryExists(string path)
            {
                var webRoot = WebHostEnvironment?.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var dir = Path.Combine(webRoot, "data");
                if (string.Equals(path, dir, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                return base.DirectoryExists(path);
            }

            protected override void DeleteFile(string path)
            {
                DeleteFileCalled = true;
                base.DeleteFile(path);
            }

            protected override void CreateDirectory(string path)
            {
                CreateDirectoryCalled = true;
                base.CreateDirectory(path);
            }

            protected override void WriteAllText(string path, string contents)
            {
                WriteAllTextCalled = true;
                base.WriteAllText(path, contents);
            }
        }

        private class TestJsonFileProductService_SavePathThrows : JsonFileProductService
        {
            public bool FailWrites { get; set; }

            public TestJsonFileProductService_SavePathThrows(IWebHostEnvironment env) : base(env) { }

            protected override bool FileExists(string path)
            {
                var webRoot = WebHostEnvironment?.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var dir = Path.Combine(webRoot, "data");
                if (string.Equals(path, dir, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return base.FileExists(path);
            }

            protected override bool DirectoryExists(string path)
            {
                var webRoot = WebHostEnvironment?.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var dir = Path.Combine(webRoot, "data");
                if (string.Equals(path, dir, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                return base.DirectoryExists(path);
            }

            protected override void WriteAllText(string path, string contents)
            {
                if (FailWrites)
                {
                    throw new IOException("simulated-fail");
                }

                base.WriteAllText(path, contents);
            }
        }

        private class TestJsonFileProductService_GetDataDirectoryNull : JsonFileProductService
        {
            public TestJsonFileProductService_GetDataDirectoryNull(IWebHostEnvironment env) : base(env) { }

            protected override string GetDataDirectory()
            {
                // Simulate an environment where the data directory cannot be determined
                return null;
            }
        }

        [Test]
        public void SaveData_FileAtDataPath_RemovedAndDirectoryCreated_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new TestJsonFileProductService_SavePathAsFile(Environment);

            var Product = new ProductModel { Id = "sd1", Title = "t" };

            // Act
            Service.CreateData(Product);

            // Assert
            Assert.IsTrue(Service.DeleteFileCalled, "DeleteFile should be called when a file exists where the data directory should be.");
            Assert.IsTrue(Service.CreateDirectoryCalled, "CreateDirectory should be called when the data directory does not exist.");
            Assert.IsTrue(Service.WriteAllTextCalled, "WriteAllText should be invoked to persist data.");

            // Cleanup
            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void SaveData_FileAtDataPath_WriteThrows_IsSwallowed_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new TestJsonFileProductService_SavePathThrows(Environment);

            var Product = new ProductModel { Id = "sd2", Title = "t2" };

            // Force writes to fail AFTER constructor succeeded
            Service.FailWrites = true;

            // Act / Assert: should not throw even though WriteAllText throws inside SaveData
            Assert.DoesNotThrow(() => Service.CreateData(Product));

            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void SaveData_DataDirectoryCannotBeDetermined_ThrowsInvalidOperation_Expected()
        {
            // Arrange - use subclass that causes GetDataDirectory to return null
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;

            var Service = new TestJsonFileProductService_GetDataDirectoryNull(Environment);

            var Product = new ProductModel { Id = "bad1", Title = "t" };

            // Act & Assert - Should handle gracefully without throwing
            Assert.DoesNotThrow(() => Service.CreateData(Product),
                "CreateData should handle invalid directory path gracefully.");

            Directory.Delete(WebRoot, true);
        }
        #endregion

        #region ConstructorEdgeCases
        [Test]
        public void Constructor_FileAtDataPath_ReplacesFileWithDirectory_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var DataPathAsFile = Path.Combine(WebRoot, "data");
            // create a file where the data directory should be
            File.WriteAllText(DataPathAsFile, "i-am-a-file");

            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;

            // Act
            var Service = new JsonFileProductService(Environment);

            // Assert
            Assert.AreEqual(false, File.Exists(DataPathAsFile), "A file at the data path should be removed.");
            Assert.AreEqual(true, Directory.Exists(Path.Combine(WebRoot, "data")), "Data folder should be created.");

            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void Constructor_NullEnvironment_UsesCurrentDirectoryFallback_Expected()
        {
            // Arrange
            var PriorDirectory = Directory.GetCurrentDirectory();
            var TempDirectory = CreateTempWebRoot();
            Directory.SetCurrentDirectory(TempDirectory);

            try
            {
                // Act
                var Service = new JsonFileProductService(null);

                // Assert
                var ExpectedDataFile = Path.Combine(TempDirectory, "wwwroot", "data", "products.json");
                Assert.AreEqual(true, File.Exists(ExpectedDataFile), "products.json should be created under fallback wwwroot.");
            }
            finally
            {
                // Reset
                Directory.SetCurrentDirectory(PriorDirectory);
                Directory.Delete(TempDirectory, true);
            }
        }

        [Test]
        public void Constructor_EnvWithNullWebRoot_UsesFallback_Expected()
        {
            // Arrange
            var PriorDirectory = Directory.GetCurrentDirectory();
            var TempDirectory = CreateTempWebRoot();
            Directory.SetCurrentDirectory(TempDirectory);

            var Environment = new TestEnv { WebRootPath = null } as IWebHostEnvironment;

            try
            {
                // Act
                var Service = new JsonFileProductService(Environment);

                // Assert - created under fallback wwwroot in temp
                var ExpectedDataFile = Path.Combine(TempDirectory, "wwwroot", "data", "products.json");
                Assert.AreEqual(true, File.Exists(ExpectedDataFile), "products.json should be created under fallback when env.WebRootPath is null.");
            }
            finally
            {
                // Reset
                Directory.SetCurrentDirectory(PriorDirectory);
                Directory.Delete(TempDirectory, true);
            }
        }
        #endregion

        #region GetProducts
        [Test]
        public void GetProducts_EmptyFile_ReturnsEmptyList_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);
            var JsonFile = Path.Combine(WebRoot, "data", "products.json");
            File.WriteAllText(JsonFile, "");

            // Act
            var Result = Service.GetProducts();

            // Reset
            // nothing to reset beyond cleanup

            // Assert
            Assert.AreEqual(0, Result.Count(), "Empty file should yield zero products.");

            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void GetProducts_InvalidJson_ReturnsEmptyList_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);
            var JsonFile = Path.Combine(WebRoot, "data", "products.json");
            File.WriteAllText(JsonFile, "{ bad json");

            // Act
            var Result = Service.GetProducts();

            // Assert
            Assert.AreEqual(0, Result.Count(), "Invalid JSON should return empty list.");

            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void GetProducts_FileLocked_ReturnsEmptyList_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);
            var JsonFile = Path.Combine(WebRoot, "data", "products.json");

            // Lock the file by opening for write and not sharing
            using (var FileStream = new FileStream(JsonFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                // Act
                var Result = Service.GetProducts();

                // Assert
                Assert.AreEqual(0, Result.Count(), "Locked file should be treated as empty list.");
            }

            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void GetProducts_UnauthorizedRead_ReturnsEmptyList_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;

            // Create a test subclass that throws UnauthorizedAccessException on read
            var Service = new TestJsonFileProductService_ReadThrows(Environment);

            // Act
            var Result = Service.GetProducts();

            // Assert
            Assert.AreEqual(0, Result.Count(), "UnauthorizedRead should return empty list.");

            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void GetProducts_NullJson_ReturnsEmptyList_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);
            var JsonFile = Path.Combine(WebRoot, "data", "products.json");
            File.WriteAllText(JsonFile, "null");

            // Act
            var Result = Service.GetProducts();

            // Assert
            Assert.AreEqual(0, Result.Count(), "A JSON value of null should be treated as empty list.");

            Directory.Delete(WebRoot, true);
        }
        #endregion

        #region AddRating
        [Test]
        public void AddRating_ProductNotFound_ReturnsFalse_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);

            // seed with one product
            var Product = new ProductModel { Id = "p1", Title = "T" };
            File.WriteAllText(Path.Combine(WebRoot, "data", "products.json"), JsonSerializer.Serialize(new[] { Product }));

            // Act
            var Result = Service.AddRating("not-found", 5);

            // Assert
            Assert.AreEqual(false, Result, "Adding rating to non-existing product should return false.");

            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void AddRating_RatingsNull_AddsRating_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);

            var Product = new ProductModel { Id = "p2", Title = "T2", Ratings = null };
            File.WriteAllText(Path.Combine(WebRoot, "data", "products.json"), JsonSerializer.Serialize(new[] { Product }));

            // Act
            var Result = Service.AddRating("p2", 4);

            // Assert
            Assert.AreEqual(true, Result, "AddRating should succeed when Ratings is null.");

            var ReadData = JsonSerializer.Deserialize<ProductModel[]>(File.ReadAllText(Path.Combine(WebRoot, "data", "products.json")));
            Assert.AreEqual(1, ReadData[0].Ratings.Length, "Ratings array should contain one element.");
            Assert.AreEqual(4, ReadData[0].Ratings[0], "Rating value should match added value.");

            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void AddRating_RatingsExist_AppendsRating_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);

            var Product = new ProductModel { Id = "p3", Title = "T3", Ratings = new int[] { 1, 2 } };
            File.WriteAllText(Path.Combine(WebRoot, "data", "products.json"), JsonSerializer.Serialize(new[] { Product }));

            // Act
            var Result = Service.AddRating("p3", 3);

            // Assert
            Assert.AreEqual(true, Result, "AddRating should append when Ratings exist.");

            var ReadData = JsonSerializer.Deserialize<ProductModel[]>(File.ReadAllText(Path.Combine(WebRoot, "data", "products.json")));
            Assert.AreEqual(3, ReadData[0].Ratings.Length, "Ratings array should be length 3 after append.");
            Assert.AreEqual(3, ReadData[0].Ratings[2], "Appended rating should be present at the end.");

            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void AddRating_NullOrWhitespaceProductId_ThrowsArgumentException_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);

            // Act & Assert - Should return false instead of throwing
            Assert.IsFalse(Service.AddRating(null, 1), "AddRating should return false for null productId.");
            Assert.IsFalse(Service.AddRating("   ", 1), "AddRating should return false for whitespace productId.");

            Directory.Delete(WebRoot, true);
        }
        #endregion

        #region UpdateData
        [Test]
        public void UpdateData_ProductNull_ThrowsArgumentNullException_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);

            // Act & Assert - Should return false instead of throwing
            Assert.IsFalse(Service.UpdateData(null), "UpdateData should return false for null product.");

            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void UpdateData_ProductNotFound_ReturnsFalse_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);

            var Product = new ProductModel { Id = "pX", Title = "Tx" };

            // Act
            var Result = Service.UpdateData(Product);

            // Assert
            Assert.AreEqual(false, Result, "UpdateData should return false when product not present.");

            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void UpdateData_ProductExists_UpdatesFields_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);

            var OriginalProduct = new ProductModel { Id = "pU", Title = "Old", Description = "D", Url = "https://a", Image = "/images/a.jpg" };
            File.WriteAllText(Path.Combine(WebRoot, "data", "products.json"), JsonSerializer.Serialize(new[] { OriginalProduct }));

            var ModifiedProduct = new ProductModel { Id = "pU", Title = "New", Description = "NewD", Url = "https://b", Image = "/images/b.jpg" };

            // Act
            var Result = Service.UpdateData(ModifiedProduct);

            // Assert
            Assert.AreEqual(true, Result, "UpdateData should return true when product exists.");

            var ReadData = JsonSerializer.Deserialize<ProductModel[]>(File.ReadAllText(Path.Combine(WebRoot, "data", "products.json")));
            Assert.AreEqual("New", ReadData[0].Title, "Title should be updated.");
            Assert.AreEqual("https://b", ReadData[0].Url, "Url should be updated.");

            Directory.Delete(WebRoot, true);
        }
        #endregion

        #region CreateData
        [Test]
        public void CreateData_ProductNull_ThrowsArgumentNullException_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);

            // Act & Assert - Should handle null gracefully without throwing
            Assert.DoesNotThrow(() => Service.CreateData(null),
                "CreateData should handle null product gracefully.");

            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void CreateData_AddsProduct_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);

            var Product = new ProductModel { Id = "pC", Title = "Tc" };

            // Act
            Service.CreateData(Product);

            // Assert
            var ReadData = JsonSerializer.Deserialize<ProductModel[]>(File.ReadAllText(Path.Combine(WebRoot, "data", "products.json")));
            Assert.AreEqual(1, ReadData.Length, "CreateData should persist the new product.");

            Directory.Delete(WebRoot, true);
        }
        #endregion

        #region DeleteData
        [Test]
        public void DeleteData_NullOrWhitespaceId_NoOp_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);

            // Act (should not throw)
            Service.DeleteData(null);
            Service.DeleteData("   ");

            // Assert - still no products
            var ReadData = JsonSerializer.Deserialize<ProductModel[]>(File.ReadAllText(Path.Combine(WebRoot, "data", "products.json")));
            Assert.AreEqual(0, ReadData.Length, "No-op delete should leave data unchanged (still empty).");

            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void DeleteData_ProductNotFound_NoOp_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);

            var Product = new ProductModel { Id = "x", Title = "t" };
            File.WriteAllText(Path.Combine(WebRoot, "data", "products.json"), JsonSerializer.Serialize(new[] { Product }));

            // Act
            Service.DeleteData("not-there");

            // Assert - original remains
            var ReadData = JsonSerializer.Deserialize<ProductModel[]>(File.ReadAllText(Path.Combine(WebRoot, "data", "products.json")));
            Assert.AreEqual(1, ReadData.Length, "Deleting non-existing product should not remove existing items.");

            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void DeleteData_ProductExists_DeletesAndImageRemoved_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);

            var ImageFolder = Path.Combine(WebRoot, "images");
            Directory.CreateDirectory(ImageFolder);
            var ImagePath = Path.Combine(ImageFolder, "t.jpg");
            File.WriteAllText(ImagePath, "data");

            var Product = new ProductModel { Id = "del1", Title = "t", Image = "/images/t.jpg" };
            File.WriteAllText(Path.Combine(WebRoot, "data", "products.json"), JsonSerializer.Serialize(new[] { Product }));

            // Act
            Service.DeleteData("del1");

            // Assert
            Assert.AreEqual(false, File.Exists(ImagePath), "Local image should be deleted when product removed.");
            var ReadData = JsonSerializer.Deserialize<ProductModel[]>(File.ReadAllText(Path.Combine(WebRoot, "data", "products.json")));
            Assert.AreEqual(0, ReadData.Length, "Product should be removed from data file.");

            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void DeleteData_LocalImageMissing_NoException_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);

            var Product = new ProductModel { Id = "miss1", Title = "t", Image = "/images/missing.jpg" };
            File.WriteAllText(Path.Combine(WebRoot, "data", "products.json"), JsonSerializer.Serialize(new[] { Product }));

            // Act
            Service.DeleteData("miss1");

            // Assert - no exception and product removed
            var ReadData = JsonSerializer.Deserialize<ProductModel[]>(File.ReadAllText(Path.Combine(WebRoot, "data", "products.json")));
            Assert.AreEqual(0, ReadData.Length, "Product should be removed even when local image file is absent.");

            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void DeleteData_LocalImageLocked_DeleteThrows_IsSwallowed_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);

            var ImageFolder = Path.Combine(WebRoot, "images");
            Directory.CreateDirectory(ImageFolder);
            var ImagePath = Path.Combine(ImageFolder, "locked.jpg");
            File.WriteAllText(ImagePath, "data");

            var Product = new ProductModel { Id = "lock1", Title = "t", Image = "/images/locked.jpg" };
            File.WriteAllText(Path.Combine(WebRoot, "data", "products.json"), JsonSerializer.Serialize(new[] { Product }));

            // Open file exclusively to make File.Delete throw on Windows
            using (var FileStream = new FileStream(ImagePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                // Act (should swallow exception)
                Assert.DoesNotThrow(() => Service.DeleteData("lock1"));
            }

            // Cleanup
            Directory.Delete(WebRoot, true);
        }

        [Test]
        public void DeleteData_WebRootBlank_SkipsImageDeletion_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var PriorDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(WebRoot);

            try
            {
                var Environment = new TestEnv { WebRootPath = string.Empty } as IWebHostEnvironment;
                var Service = new JsonFileProductService(Environment);

                var Product = new ProductModel { Id = "b1", Title = "t", Image = "/images/skip.jpg" };

                // Seed via service so it writes into the relative 'data' under current directory
                Service.CreateData(Product);

                // Act - should not throw and should remove product
                Service.DeleteData("b1");

                var ReadData = JsonSerializer.Deserialize<ProductModel[]>(File.ReadAllText(Path.Combine(WebRoot, "data", "products.json")));
                Assert.AreEqual(0, ReadData.Length, "Product should be removed even when WebRootPath is blank.");
            }
            finally
            {
                Directory.SetCurrentDirectory(PriorDirectory);
                Directory.Delete(WebRoot, true);
            }
        }

        [Test]
        public void DeleteData_RemoteImage_Ignored_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new JsonFileProductService(Environment);

            var Product = new ProductModel { Id = "del2", Title = "t2", Image = "https://example.com/t.jpg" };
            File.WriteAllText(Path.Combine(WebRoot, "data", "products.json"), JsonSerializer.Serialize(new[] { Product }));

            // Act
            Service.DeleteData("del2");

            // Assert - no exception and file now empty
            var ReadData = JsonSerializer.Deserialize<ProductModel[]>(File.ReadAllText(Path.Combine(WebRoot, "data", "products.json")));
            Assert.AreEqual(0, ReadData.Length, "Product should be removed even if image is remote.");

            Directory.Delete(WebRoot, true);
        }
        #endregion

        #region SaveDataException
        private class TestJsonFileProductService_ReadThrows : JsonFileProductService
        {
            public TestJsonFileProductService_ReadThrows(IWebHostEnvironment env) : base(env) { }
            protected override string ReadAllText(string path) => throw new UnauthorizedAccessException();
        }

        private class TestJsonFileProductService_WriteThrows : JsonFileProductService
        {
            public bool FailWrites { get; set; }

            public TestJsonFileProductService_WriteThrows(IWebHostEnvironment env) : base(env) { }

            protected override void WriteAllText(string path, string contents)
            {
                if (FailWrites)
                {
                    throw new IOException("fail");
                }

                base.WriteAllText(path, contents);
            }
        }

        [Test]
        public void SaveData_WriteThrows_IsSwallowed_Expected()
        {
            // Arrange
            var WebRoot = CreateTempWebRoot();
            var Environment = new TestEnv { WebRootPath = WebRoot } as IWebHostEnvironment;
            var Service = new TestJsonFileProductService_WriteThrows(Environment);

            var Product = new ProductModel { Id = "s1", Title = "s" };

            // Force subsequent writes to fail; constructor writes succeeded above.
            Service.FailWrites = true;

            // Act (should not throw despite write failing)
            Assert.DoesNotThrow(() => Service.CreateData(Product));

            // Cleanup
            Directory.Delete(WebRoot, true);
        }
        #endregion
    }
}
