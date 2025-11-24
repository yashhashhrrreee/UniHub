using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace ContosoCrafts.WebSite
{
    /// <summary>
    /// Entry point of the ContosoCrafts web application.
    /// Responsible for configuring and launching the web host.
    /// Includes startup exception handling for safer application bootstrapping.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Program
    {
        /// <summary>
        /// Application entry point.
        /// Initializes and starts the ASP.NET Core web host.
        /// </summary>
        /// <param name="args">Command-line arguments passed to the application.</param>
        public static void Main(string[] args)
        {
            // Defensive: ensure args is not null to avoid potential null reference usage.
            if (args == null)
            {
                args = Array.Empty<string>();
            }

            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Configures the host builder for the ASP.NET Core web application.
        /// This method applies default configurations and specifies the Startup class.
        /// </summary>
        /// <param name="args">Command-line arguments for host configuration.</param>
        /// <returns>A configured instance of <see cref="IHostBuilder"/>.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            // Defensive: ensure args is not null
            if (args == null)
            {
                args = Array.Empty<string>();
            }

            // Build and configure the host for ASP.NET Core.
            var HostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

            return HostBuilder;
        }
    }
}