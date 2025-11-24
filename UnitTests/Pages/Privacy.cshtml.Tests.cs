using ContosoCrafts.WebSite.Pages;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTests.Pages.Privacy
{
    /// <summary>
    /// Unit tests for the <see cref="PrivacyModel"/> page model.
    /// </summary>
    [TestFixture]
    public class PrivacyTests
    {
        #region OnGet

        /// <summary>
        /// PrivacyModel_OnGet_SetsModelStateValid_ExpectedTrue
        /// </summary>
        [Test]
        public void PrivacyModel_OnGet_SetsModelStateValid_ExpectedTrue()
        {
            // Arrange
            var MockLogger = Mock.Of<ILogger<PrivacyModel>>();

            var privacyModel = new PrivacyModel(MockLogger);

            // Act
            privacyModel.OnGet();

            var IsModelStateValid = privacyModel.ModelState.IsValid;

            // Reset
            // Clear references to help GC in test environment
            privacyModel = null;
            MockLogger = null;

            // Assert
            Assert.AreEqual(true, IsModelStateValid, "ModelState should be valid after OnGet.");
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Tests that constructor throws ArgumentNullException for null logger.
        /// </summary>
        [Test]
        public void Constructor_NullLogger_ThrowsArgumentNullException_ExpectedThrows()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => new PrivacyModel(null), "Constructor should throw ArgumentNullException for null logger.");
        }

        #endregion
    }
}
