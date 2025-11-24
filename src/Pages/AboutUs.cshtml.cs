using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoCrafts.WebSite.Pages
{

    /// <summary>
    /// Page model for the About Us page. Handles any server-side logic for the About view.
    /// </summary>
    public class AboutModel : PageModel
    {

        /// <summary>
        /// Handles GET requests for the About Us page.
        /// Currently performs no additional logic, but can be extended 
        /// in the future to include data loading or telemetry.
        /// </summary>
        public void OnGet()
        {
            // No specific logic needed for now
        }

    }
}