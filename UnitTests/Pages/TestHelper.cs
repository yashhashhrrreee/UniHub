using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

using Moq;

using ContosoCrafts.WebSite.Services;

namespace UnitTests
{
    /// <summary>
    /// Helper to hold the web start settings
    ///
    /// HttpClient
    /// 
    /// Action Context
    /// 
    /// The View Data and Temp Data
    /// 
    /// The Product Service
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Mock web hosting environment for testing
        /// </summary>
        public static Mock<IWebHostEnvironment> MockWebHostEnvironment;

        /// <summary>
        /// URL helper factory for generating URLs
        /// </summary>
        public static IUrlHelperFactory UrlHelperFactory;

        /// <summary>
        /// Default HTTP context for testing
        /// </summary>
        public static DefaultHttpContext HttpContextDefault;

        /// <summary>
        /// Web hosting environment interface
        /// </summary>
        public static IWebHostEnvironment WebHostEnvironment;

        /// <summary>
        /// Model state dictionary for validation
        /// </summary>
        public static ModelStateDictionary ModelState;

        /// <summary>
        /// Action context for controller actions
        /// </summary>
        public static ActionContext ActionContext;

        /// <summary>
        /// Empty model metadata provider for testing
        /// </summary>
        public static EmptyModelMetadataProvider ModelMetadataProvider;

        /// <summary>
        /// View data dictionary for passing data to views
        /// </summary>
        public static ViewDataDictionary ViewData;

        /// <summary>
        /// Temp data dictionary for temporary data storage
        /// </summary>
        public static TempDataDictionary TempData;

        /// <summary>
        /// Page context for Razor Pages
        /// </summary>
        public static PageContext PageContext;

        /// <summary>
        /// Product service for data operations
        /// </summary>
        public static JsonFileProductService ProductService;

        /// <summary>
        /// Default Constructor
        /// </summary>
        static TestHelper()
        {
            MockWebHostEnvironment = new Mock<IWebHostEnvironment>();

            MockWebHostEnvironment.Setup(m => m.EnvironmentName).Returns("Hosting:UnitTestEnvironment");
            MockWebHostEnvironment.Setup(m => m.WebRootPath).Returns(TestFixture.DataWebRootPath);
            MockWebHostEnvironment.Setup(m => m.ContentRootPath).Returns(TestFixture.DataContentRootPath);

            HttpContextDefault = new DefaultHttpContext()
            {
                TraceIdentifier = "trace",
            };

            HttpContextDefault.HttpContext.TraceIdentifier = "trace";

            ModelState = new ModelStateDictionary();

            ActionContext = new ActionContext(HttpContextDefault, HttpContextDefault.GetRouteData(), new PageActionDescriptor(), ModelState);

            ModelMetadataProvider = new EmptyModelMetadataProvider();

            ViewData = new ViewDataDictionary(ModelMetadataProvider, ModelState);

            TempData = new TempDataDictionary(HttpContextDefault, Mock.Of<ITempDataProvider>());

            PageContext = new PageContext(ActionContext)
            {
                ViewData = ViewData,
                HttpContext = HttpContextDefault
            };

            ProductService = new JsonFileProductService(MockWebHostEnvironment.Object);
        }
    }
}