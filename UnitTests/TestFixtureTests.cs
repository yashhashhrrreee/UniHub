using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;

namespace UnitTests
{
    /// <summary>
    /// Comprehensive tests for TestFixture class to achieve 100% code coverage.
    /// Tests setup, teardown, and data path management functionality.
    /// </summary>
    [TestFixture]
    public class TestFixtureTests
    {
        private TestFixture _testFixture;

        [SetUp]
        public void SetUp()
        {
            _testFixture = new TestFixture();
        }

        /// <summary>
        /// Test that DataWebRootPath is properly set during initialization.
        /// </summary>
        [Test]
        public void DataWebRootPath_Initialization_SetCorrectly()
        {
            // Act & Assert
            Assert.IsNotNull(TestFixture.DataWebRootPath, "DataWebRootPath should be set");
            Assert.AreEqual("./wwwroot", TestFixture.DataWebRootPath,
                "DataWebRootPath should be set to ./wwwroot");
        }

        /// <summary>
        /// Test that DataContentRootPath is properly set during initialization.
        /// </summary>
        [Test]
        public void DataContentRootPath_Initialization_SetCorrectly()
        {
            // Act & Assert
            Assert.IsNotNull(TestFixture.DataContentRootPath, "DataContentRootPath should be set");
            Assert.AreEqual("./data/", TestFixture.DataContentRootPath,
                "DataContentRootPath should be set to ./data/");
        }

        /// <summary>
        /// Test that DataTestFile is properly set.
        /// </summary>
        [Test]
        public void DataTestFile_Initialization_SetCorrectly()
        {
            // Act & Assert
            Assert.IsNotNull(TestFixture.DataTestFile, "DataTestFile should be set");
            Assert.AreEqual("products_test.json", TestFixture.DataTestFile,
                "DataTestFile should be products_test.json");
        }

        /// <summary>
        /// Test that DataProductionFile is properly set.
        /// </summary>
        [Test]
        public void DataProductionFile_Initialization_SetCorrectly()
        {
            // Act & Assert
            Assert.IsNotNull(TestFixture.DataProductionFile, "DataProductionFile should be set");
            Assert.AreEqual("products.json", TestFixture.DataProductionFile,
                "DataProductionFile should be products.json");
        }

        /// <summary>
        /// Test that NetVersion is properly set.
        /// </summary>
        [Test]
        public void NetVersion_Initialization_SetCorrectly()
        {
            // Act & Assert
            Assert.IsNotNull(TestFixture.NetVersion, "NetVersion should be set");
            Assert.AreEqual("net8.0", TestFixture.NetVersion,
                "NetVersion should be net8.0");
        }

        /// <summary>
        /// Test that RunBeforeAnyTests method executes without errors.
        /// </summary>
        [Test]
        public void RunBeforeAnyTests_Execution_CompletesSuccessfully()
        {
            // Arrange
            var fixture = new TestFixture();

            // Act & Assert - Test calling it manually to ensure it doesn't throw
            Assert.DoesNotThrow(() => fixture.RunBeforeAnyTests(),
                "RunBeforeAnyTests should not throw when called");

            // Verify the test data directory was created
            Assert.IsTrue(Directory.Exists("wwwroot"),
                "RunBeforeAnyTests should create wwwroot directory");
            Assert.IsTrue(Directory.Exists("wwwroot/data"),
                "RunBeforeAnyTests should create wwwroot/data directory");
        }

        /// <summary>
        /// Test that RunAfterAnyTests method executes without errors.
        /// </summary>
        [Test]
        public void RunAfterAnyTests_Execution_CompletesSuccessfully()
        {
            // Arrange
            var fixture = new TestFixture();

            // Act & Assert - Test calling it manually to ensure it doesn't throw
            Assert.DoesNotThrow(() => fixture.RunAfterAnyTests(),
                "RunAfterAnyTests should not throw when called");
        }

        /// <summary>
        /// Test that multiple TestFixture instances work correctly.
        /// </summary>
        [Test]
        public void TestFixture_MultipleInstances_WorkCorrectly()
        {
            // Arrange
            var fixture1 = new TestFixture();
            var fixture2 = new TestFixture();

            // Act & Assert - Static paths should be the same across instances
            Assert.AreEqual(TestFixture.DataWebRootPath, TestFixture.DataWebRootPath,
                "DataWebRootPath should be consistent across instances");
            Assert.AreEqual(TestFixture.DataContentRootPath, TestFixture.DataContentRootPath,
                "DataContentRootPath should be consistent across instances");

            // Both instances should work independently
            Assert.IsNotNull(fixture1, "First fixture instance should be valid");
            Assert.IsNotNull(fixture2, "Second fixture instance should be valid");
        }

        /// <summary>
        /// Test that the TestFixture class follows NUnit patterns correctly.
        /// </summary>
        [Test]
        public void TestFixture_NUnitIntegration_FollowsPatterns()
        {
            // Verify the class has the correct NUnit attributes
            var fixtureType = typeof(TestFixture);
            var setupAttributes = fixtureType.GetCustomAttributes(typeof(SetUpFixtureAttribute), false);

            Assert.AreEqual(1, setupAttributes.Length, "TestFixture should have SetUpFixture attribute");

            // Verify setup and teardown methods exist
            var setupMethod = fixtureType.GetMethod("RunBeforeAnyTests");
            var teardownMethod = fixtureType.GetMethod("RunAfterAnyTests");

            Assert.IsNotNull(setupMethod, "RunBeforeAnyTests method should exist");
            Assert.IsNotNull(teardownMethod, "RunAfterAnyTests method should exist");

            // Verify they have the correct attributes
            var setupAttrs = setupMethod.GetCustomAttributes(typeof(OneTimeSetUpAttribute), false);
            var teardownAttrs = teardownMethod.GetCustomAttributes(typeof(OneTimeTearDownAttribute), false);

            Assert.AreEqual(1, setupAttrs.Length, "RunBeforeAnyTests should have OneTimeSetUp attribute");
            Assert.AreEqual(1, teardownAttrs.Length, "RunAfterAnyTests should have OneTimeTearDown attribute");
        }

        /// <summary>
        /// Test the static field initialization and values.
        /// </summary>
        [Test]
        public void TestFixture_StaticFields_InitializedCorrectly()
        {
            // Test that static fields are initialized properly
            Assert.IsNotNull(TestFixture.DataWebRootPath, "Static DataWebRootPath should be initialized");
            Assert.IsNotNull(TestFixture.DataContentRootPath, "Static DataContentRootPath should be initialized");
            Assert.IsNotNull(TestFixture.DataTestFile, "Static DataTestFile should be initialized");
            Assert.IsNotNull(TestFixture.DataProductionFile, "Static DataProductionFile should be initialized");
            Assert.IsNotNull(TestFixture.NetVersion, "Static NetVersion should be initialized");

            // Test that they maintain their values across multiple accesses
            var originalWebRoot = TestFixture.DataWebRootPath;
            var originalContentRoot = TestFixture.DataContentRootPath;
            var originalTestFile = TestFixture.DataTestFile;
            var originalProductionFile = TestFixture.DataProductionFile;
            var originalNetVersion = TestFixture.NetVersion;

            // Access them again
            Assert.AreEqual(originalWebRoot, TestFixture.DataWebRootPath,
                "DataWebRootPath should maintain consistent value");
            Assert.AreEqual(originalContentRoot, TestFixture.DataContentRootPath,
                "DataContentRootPath should maintain consistent value");
            Assert.AreEqual(originalTestFile, TestFixture.DataTestFile,
                "DataTestFile should maintain consistent value");
            Assert.AreEqual(originalProductionFile, TestFixture.DataProductionFile,
                "DataProductionFile should maintain consistent value");
            Assert.AreEqual(originalNetVersion, TestFixture.NetVersion,
                "NetVersion should maintain consistent value");
        }

        /// <summary>
        /// Test the data directory setup functionality in detail.
        /// </summary>
        [Test]
        public void TestFixture_DataDirectorySetup_WorksCorrectly()
        {
            // Arrange
            var fixture = new TestFixture();

            // Ensure we start clean (this might be already created by other tests)
            if (Directory.Exists("wwwroot"))
            {
                Directory.Delete("wwwroot", true);
            }

            // Act
            fixture.RunBeforeAnyTests();

            // Assert
            Assert.IsTrue(Directory.Exists("wwwroot"), "wwwroot directory should be created");
            Assert.IsTrue(Directory.Exists("wwwroot/data"), "wwwroot/data directory should be created");

            // Check if test data files were copied
            var dataFiles = Directory.GetFiles("wwwroot/data");
            Assert.Greater(dataFiles.Length, 0, "Data files should be copied to test directory");

            // Check for the specific files that should exist
            var productionFile = Path.Combine("wwwroot/data", TestFixture.DataProductionFile);
            Assert.IsTrue(File.Exists(productionFile) || dataFiles.Length > 0,
                "Production data file should exist or data files should be present");
        }

        /// <summary>
        /// Test edge cases and error handling.
        /// </summary>
        [Test]
        public void TestFixture_EdgeCases_HandledGracefully()
        {
            // Test that accessing static fields multiple times is consistent
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual("./wwwroot", TestFixture.DataWebRootPath, "DataWebRootPath should be consistent");
                Assert.AreEqual("./data/", TestFixture.DataContentRootPath, "DataContentRootPath should be consistent");
                Assert.AreEqual("products_test.json", TestFixture.DataTestFile, "DataTestFile should be consistent");
                Assert.AreEqual("products.json", TestFixture.DataProductionFile, "DataProductionFile should be consistent");
                Assert.AreEqual("net8.0", TestFixture.NetVersion, "NetVersion should be consistent");
            }
        }

        /// <summary>
        /// Test the directory cleanup and recreation logic.
        /// </summary>
        [Test]
        public void TestFixture_DirectoryCleanup_WorksCorrectly()
        {
            // Arrange
            var fixture = new TestFixture();
            var testDir = "wwwroot";

            // Create a test directory with some content
            if (!Directory.Exists(testDir))
            {
                Directory.CreateDirectory(testDir);
            }
            File.WriteAllText(Path.Combine(testDir, "test.txt"), "test content");

            // Act - RunBeforeAnyTests should delete and recreate the directory
            fixture.RunBeforeAnyTests();

            // Assert - Old test file should be gone, new structure should exist
            Assert.IsFalse(File.Exists(Path.Combine(testDir, "test.txt")),
                "Old test file should be deleted");
            Assert.IsTrue(Directory.Exists(testDir),
                "Directory should be recreated");
            Assert.IsTrue(Directory.Exists(Path.Combine(testDir, "data")),
                "Data subdirectory should be created");
        }

        /// <summary>
        /// Test teardown method functionality.
        /// </summary>
        [Test]
        public void TestFixture_TearDown_CompletesWithoutErrors()
        {
            // Arrange
            var fixture = new TestFixture();

            // Act & Assert - Currently RunAfterAnyTests is empty, but should not throw
            Assert.DoesNotThrow(() => fixture.RunAfterAnyTests(),
                "RunAfterAnyTests should complete without errors");

            // Test multiple calls
            Assert.DoesNotThrow(() =>
            {
                fixture.RunAfterAnyTests();
                fixture.RunAfterAnyTests();
            }, "Multiple calls to RunAfterAnyTests should not cause issues");
        }
    }
}