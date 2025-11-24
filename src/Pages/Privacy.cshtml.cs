using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ContosoCrafts.WebSite.Pages
{
    /// <summary>
    /// Page model for the Privacy page.
    /// Displays the site's privacy and data-use information.
    /// </summary>
    public class PrivacyModel : PageModel
    {
        // Logger used for page diagnostics.
        private readonly ILogger<PrivacyModel> Logger;


        /// <summary>
        /// Initializes a new instance of the <see cref="PrivacyModel"/> class.
        /// Uses fast-fail to require a non-null logger instance.
        /// </summary>
        /// <param name="logger">Injected logger used for page diagnostics.</param>
        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            Logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles GET requests for the Privacy page.
        /// Currently no dynamic data is loaded, but this method can be extended
        /// to include future logic such as audit logging or analytics tracking.
        /// </summary>
        public void OnGet()
        {
        }
    }
}
