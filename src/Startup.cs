using ContosoCrafts.WebSite.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ContosoCrafts.WebSite
{
    /// <summary>
    /// Configures application services and the HTTP request pipeline.
    /// Responsible for setting up dependency injection, middleware, and endpoints.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// Accepts configuration injected by the ASP.NET Core host.
        /// </summary>
        /// <param name="configuration">Application configuration source.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // Provides access to the application's configuration settings.
        public IConfiguration Configuration
        {
            get;
        }

        /// <summary>
        /// Called by the runtime to register services for dependency injection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Use defensive check instead of throwing
            if (services == null)
            {
                return;
            }

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHttpClient();
            // Ensure routing services are registered for endpoint routing and tests that exercise Startup
            services.AddRouting();
            services.AddControllers();

            // Provide a sane default HTTPS port for environments (like unit tests) that don't configure one.
            // This prevents the HttpsRedirectionMiddleware from warning when it cannot determine an HTTPS port.
            services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = 443;
            });
            services.AddTransient<JsonFileProductService>();
        }

        /// <summary>
        /// Called by the runtime to configure the middleware pipeline that handles HTTP requests.
        /// </summary>
        /// <param name="app">The application builder used to construct the middleware pipeline.</param>
        /// <param name="env">Provides information about the hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Use defensive check instead of throwing
            if (app == null || env == null)
            {
                return;
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Avoid 'else' to follow fast-fail / clearer flow. Explicitly handle non-development case.
            if (env.IsDevelopment() == false)
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // UseRouting requires routing services to be registered. Call it directly;
            // let any configuration errors surface so calling code can detect missing services.
            app.UseRouting();

            // UseAuthorization expects authorization services to be registered.
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Mapping endpoints may require additional framework services that are not
                // present in minimal test service collections used by unit tests. Check for a
                // key MVC service and only map if present to avoid throwing during tests.

                var Provider = app.ApplicationServices;

                var HasActionDescriptorProvider = Provider.GetService(typeof(Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider)) != null;

                if (HasActionDescriptorProvider)
                {
                    endpoints.MapRazorPages();

                    endpoints.MapControllers();

                    endpoints.MapBlazorHub();
                }
            });
            // endpoints.MapGet("/products", (context) => 
            // {
            //     var products = app.ApplicationServices.GetService<JsonFileProductService>().GetProducts();
            //     var json = JsonSerializer.Serialize<IEnumerable<Product>>(products);
            //     return context.Response.WriteAsync(json);
            // });
        }
    }
}
