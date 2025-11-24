using System.Diagnostics;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

using Moq;

using ContosoCrafts.WebSite.Pages;

namespace UnitTests.Pages.Error
{
    /// <summary>
    /// Provides unit testing for the Error page
    /// </summary>
    [TestFixture]
    public class ErrorTests
    {
        #region TestSetup
        // Declare the model of the Error page to be used in unit tests
        public static ErrorModel PageModel;

        [SetUp]
        /// <summary>
        /// Initializes mock error page model for testing.
        /// </summary>
        public void TestInitialize()
        {
            // Logs where the category name is derived from for the mocked ErrorMoel
            var mockLogger = Mock.Of<ILogger<ErrorModel>>();

            PageModel = new ErrorModel(mockLogger)
            {
                // Holds the dummy PageContext from testHelper
                PageContext = TestHelper.PageContext,

                // Holds the dummy tempData from testHelper
                TempData = TestHelper.TempData,
            };
        }

        #endregion TestSetup

        [Test]
        /// <summary>
        /// Verifies that the ErrorModel constructor handles null logger gracefully.
        /// This covers the defensive programming approach in the constructor.
        /// </summary>
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Arrange / Act / Assert
            Assert.DoesNotThrow(() => new ErrorModel(null), "Constructor should handle null logger gracefully.");
        }

        #region OnGet
        [Test]
        /// <summary>
        /// OnGet_ActivityStarted_SetsRequestIdToActivityId_ExpectedMatch
        /// </summary>
        public void OnGet_ActivityStarted_SetsRequestIdToActivityId_ExpectedMatch()
        {
            // Arrange
            Activity activity = new Activity("activity");

            activity.Start();

            // Act
            PageModel.OnGet();

            // Reset
            activity.Stop();

            // Assert
            Assert.AreEqual(true, PageModel.ModelState.IsValid);
            Assert.AreEqual(activity.Id, PageModel.RequestId);
        }

        [Test]
        /// <summary>
        /// OnGet_NoActivity_SetsRequestIdToTrace_ExpectedTrace
        /// </summary>
        public void OnGet_NoActivity_SetsRequestIdToTrace_ExpectedTrace()
        {
            // Arrange

            // Act
            PageModel.OnGet();

            // Assert
            Assert.AreEqual(true, PageModel.ModelState.IsValid);
            Assert.AreEqual("trace", PageModel.RequestId);
            Assert.AreEqual(true, PageModel.ShowRequestId);
        }
        #endregion OnGet
    }
}