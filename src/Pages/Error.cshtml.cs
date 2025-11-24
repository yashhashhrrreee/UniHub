using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ContosoCrafts.WebSite.Pages
{
    /// <summary>
    /// Page model for handling and displaying error information to the user.
    /// Implements diagnostic logic to capture request identifiers for logging and troubleshooting.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        // Unique identifier for the current request.
        public string RequestId
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the <see cref="RequestId"/> should be displayed on the error page.
        /// </summary>
        public bool ShowRequestId => string.IsNullOrEmpty(RequestId) == false;

        // Logger injected for recording diagnostic information.
        private readonly ILogger<ErrorModel> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorModel"/> class with a logger dependency.
        /// </summary>
        /// <param name="logger">Injected logger for recording diagnostic information.</param>
        public ErrorModel(ILogger<ErrorModel> logger)
        {
            Logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<ErrorModel>.Instance;
        }

        /// <summary>
        /// Handles GET requests to the Error page.
        /// Captures the request ID from the current activity or HTTP context
        /// to aid in debugging and error tracing.
        /// </summary>
        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }

    }
}