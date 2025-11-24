using ContosoCrafts.WebSite.Pages;
using NUnit.Framework;

namespace UnitTests.Pages.About
{
    /// <summary>
    /// Unit tests for the <see cref="AboutModel"/> page model.
    /// </summary>
    [TestFixture]
    public class AboutUsTests
    {
        #region OnGet

        /// <summary>
        /// AboutModel_OnGet_SetsModelStateValid_ExpectedTrue
        /// </summary>
        [Test]
        public void AboutModel_OnGet_SetsModelStateValid_ExpectedTrue()
        {
            // Arrange
            var aboutModel = new AboutModel();

            // Act
            aboutModel.OnGet();

            var isValid = aboutModel.ModelState.IsValid;

            // Reset
            aboutModel = null;

            // Assert
            Assert.IsTrue(isValid, "ModelState should be valid after OnGet.");
        }

        #endregion
    }
}
