using System.IO;
using ContosoCrafts.WebSite.Controllers;
using NUnit.Framework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace UnitTests.Controllers
{
    /// <summary>
    /// Unit tests for MockWebHostEnvironment functionality.
    /// </summary>
    [TestFixture]
    public class MockWebHostEnvironmentTests
    {
        #region Tests

        /// <summary>
        /// Test that MockWebHostEnvironment can be instantiated with default values.
        /// </summary>
        [Test]
        public void Constructor_Default_SetsExpectedDefaultValues()
        {
            // Act
            var mockEnvironment = new MockWebHostEnvironment();

            // Assert
            Assert.IsNotNull(mockEnvironment, "MockWebHostEnvironment should not be null.");
            Assert.AreEqual(Path.GetTempPath(), mockEnvironment.WebRootPath, "WebRootPath should be temp path by default.");
            Assert.AreEqual("ContosoCrafts.WebSite", mockEnvironment.ApplicationName, "ApplicationName should be set to ContosoCrafts.WebSite.");
            Assert.AreEqual(Path.GetTempPath(), mockEnvironment.ContentRootPath, "ContentRootPath should be temp path by default.");
            Assert.AreEqual("Development", mockEnvironment.EnvironmentName, "EnvironmentName should be Development by default.");
        }

        /// <summary>
        /// Test that all properties can be set and retrieved.
        /// </summary>
        [Test]
        public void Properties_CanSetAndGet_AllProperties()
        {
            // Arrange
            var mockEnvironment = new MockWebHostEnvironment();
            var testWebRootPath = @"C:\TestWebRoot";
            var testApplicationName = "TestApp";
            var testContentRootPath = @"C:\TestContentRoot";
            var testEnvironmentName = "Testing";
            var testFileProvider = new NullFileProvider();

            // Act
            mockEnvironment.WebRootPath = testWebRootPath;
            mockEnvironment.ApplicationName = testApplicationName;
            mockEnvironment.ContentRootPath = testContentRootPath;
            mockEnvironment.EnvironmentName = testEnvironmentName;
            mockEnvironment.WebRootFileProvider = testFileProvider;
            mockEnvironment.ContentRootFileProvider = testFileProvider;

            // Assert
            Assert.AreEqual(testWebRootPath, mockEnvironment.WebRootPath, "WebRootPath should be settable.");
            Assert.AreEqual(testApplicationName, mockEnvironment.ApplicationName, "ApplicationName should be settable.");
            Assert.AreEqual(testContentRootPath, mockEnvironment.ContentRootPath, "ContentRootPath should be settable.");
            Assert.AreEqual(testEnvironmentName, mockEnvironment.EnvironmentName, "EnvironmentName should be settable.");
            Assert.AreEqual(testFileProvider, mockEnvironment.WebRootFileProvider, "WebRootFileProvider should be settable.");
            Assert.AreEqual(testFileProvider, mockEnvironment.ContentRootFileProvider, "ContentRootFileProvider should be settable.");
        }

        /// <summary>
        /// Test that MockWebHostEnvironment can be used as IWebHostEnvironment.
        /// </summary>
        [Test]
        public void MockWebHostEnvironment_ImplementsInterface_IWebHostEnvironment()
        {
            // Act
            IWebHostEnvironment environment = new MockWebHostEnvironment();

            // Assert
            Assert.IsNotNull(environment, "MockWebHostEnvironment should implement IWebHostEnvironment.");
            Assert.IsInstanceOf<IWebHostEnvironment>(environment, "Should be assignable to IWebHostEnvironment interface.");
        }

        /// <summary>
        /// Test that WebRootFileProvider can be null.
        /// </summary>
        [Test]
        public void WebRootFileProvider_CanBeNull_WithoutException()
        {
            // Arrange
            var mockEnvironment = new MockWebHostEnvironment();

            // Act & Assert
            Assert.DoesNotThrow(() => mockEnvironment.WebRootFileProvider = null, "Setting WebRootFileProvider to null should not throw.");
            Assert.IsNull(mockEnvironment.WebRootFileProvider, "WebRootFileProvider should be null when set to null.");
        }

        /// <summary>
        /// Test that ContentRootFileProvider can be null.
        /// </summary>
        [Test]
        public void ContentRootFileProvider_CanBeNull_WithoutException()
        {
            // Arrange
            var mockEnvironment = new MockWebHostEnvironment();

            // Act & Assert
            Assert.DoesNotThrow(() => mockEnvironment.ContentRootFileProvider = null, "Setting ContentRootFileProvider to null should not throw.");
            Assert.IsNull(mockEnvironment.ContentRootFileProvider, "ContentRootFileProvider should be null when set to null.");
        }

        #endregion
    }
}