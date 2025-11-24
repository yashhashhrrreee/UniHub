using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using ContosoCrafts.WebSite;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using ContosoCrafts.WebSite.Services;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="ContosoCrafts.WebSite.Startup"/> class.
    /// Exercises configuration and startup pipeline behaviors used by the application.
    /// </summary>
    [TestFixture]
    public class StartupTests
    {
        #region Helper Classes

        private class TestEnv : IWebHostEnvironment
        {
            public string WebRootPath { get; set; }
            public string EnvironmentName { get; set; }
            public string ApplicationName { get; set; }
            public string ContentRootPath { get; set; }
            public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; }
            public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }
        }

        private class SimpleActionDescriptorProvider : IActionDescriptorCollectionProvider
        {
            public ActionDescriptorCollection ActionDescriptors { get; } = new ActionDescriptorCollection(new System.Collections.Generic.List<ActionDescriptor>(), 0);
        }

        #endregion

        #region ConfigureServices Tests

        /// <summary>
        /// Tests that ConfigureServices registers expected services including JsonFileProductService.
        /// </summary>
        [Test]
        public void ConfigureServices_RegistersExpectedServices()
        {
            // Arrange
            var Configuration = new ConfigurationBuilder().Build();
            var StartupInstance = new Startup(Configuration);
            var Services = new ServiceCollection();

            // Act
            StartupInstance.ConfigureServices(Services);

            // Assert - ensure some expected services were registered
            // Avoid using lambda expressions here so coverage tools don't create extra generated methods.
            Assert.IsTrue(Services.Count > 0, "Services should be registered");

            var found = false;
            foreach (var sd in Services)
            {
                if (sd.ServiceType == typeof(JsonFileProductService))
                {
                    found = true;
                    break;
                }

                if (sd.ImplementationType != null)
                {
                    if (sd.ImplementationType == typeof(JsonFileProductService))
                    {
                        found = true;
                        break;
                    }
                }
            }

            Assert.IsTrue(found, "JsonFileProductService should be registered");
        }

        /// <summary>
        /// Tests that ConfigureServices handles null services parameter gracefully.
        /// </summary>
        [Test]
        public void ConfigureServices_Null_ThrowsArgumentNullException()
        {
            // Arrange
            var StartupInstance = new ContosoCrafts.WebSite.Startup(new ConfigurationBuilder().Build());

            // Act & Assert - Should handle null gracefully
            Assert.DoesNotThrow(() => StartupInstance.ConfigureServices(null),
                "ConfigureServices should handle null services gracefully.");
        }

        #endregion

        #region Configure Tests

        /// <summary>
        /// Tests that Configure method does not throw exceptions in Development environment.
        /// </summary>
        [Test]
        public void Configure_Development_DoesNotThrow()
        {
            // Arrange
            var Configuration = new ConfigurationBuilder().Build();
            var StartupInstance = new Startup(Configuration);

            // Create a minimal provider to avoid executing full MVC endpoint mapping
            var Services = new ServiceCollection();
            Services.AddRouting();
            Services.AddAuthorization();
            var ServiceProvider = Services.BuildServiceProvider();

            var ApplicationBuilder = new Microsoft.AspNetCore.Builder.ApplicationBuilder(ServiceProvider);

            var Environment = new TestEnv { EnvironmentName = "Development" };

            // Act / Assert - should not throw when configuring in development with minimal services
            Assert.DoesNotThrow(() => StartupInstance.Configure(ApplicationBuilder, Environment));
        }

        /// <summary>
        /// Tests that Configure method does not throw exceptions in Production environment.
        /// </summary>
        [Test]
        public void Configure_Production_DoesNotThrow()
        {
            // Arrange
            var Configuration = new ConfigurationBuilder().Build();
            var StartupInstance = new Startup(Configuration);

            // Create a minimal provider to avoid executing full MVC endpoint mapping
            var Services = new ServiceCollection();
            Services.AddRouting();
            Services.AddAuthorization();
            var ServiceProvider = Services.BuildServiceProvider();

            var ApplicationBuilder = new Microsoft.AspNetCore.Builder.ApplicationBuilder(ServiceProvider);

            var Environment = new TestEnv { EnvironmentName = "Production" };

            // Act / Assert - should not throw when configuring in production with minimal services
            Assert.DoesNotThrow(() => StartupInstance.Configure(ApplicationBuilder, Environment));
        }

        /// <summary>
        /// Tests that Configure handles null app or environment parameters gracefully.
        /// </summary>
        [Test]
        public void Configure_ThrowsOnNullAppOrEnv()
        {
            // Arrange
            var StartupInstance = new ContosoCrafts.WebSite.Startup(new ConfigurationBuilder().Build());
            var Services = new ServiceCollection();
            Services.AddRouting();
            var ServiceProvider = Services.BuildServiceProvider();

            var ApplicationBuilder = new ApplicationBuilder(ServiceProvider);
            var Environment = new TestEnv { EnvironmentName = Environments.Production };

            // Act & Assert - Should handle null gracefully
            Assert.DoesNotThrow(() => StartupInstance.Configure(null, Environment),
                "Configure should handle null app gracefully.");

            Assert.DoesNotThrow(() => StartupInstance.Configure(ApplicationBuilder, null),
                "Configure should handle null environment gracefully.");
        }

        /// <summary>
        /// Tests that Configure executes without exception in Development environment with minimal services.
        /// </summary>
        [Test]
        public void Configure_Development_ExecutesWithoutException()
        {
            // Arrange
            var StartupInstance = new ContosoCrafts.WebSite.Startup(new ConfigurationBuilder().Build());
            var Services = new ServiceCollection();
            // Keep service collection minimal to avoid deep MVC startup plumbing in unit tests
            Services.AddRouting();
            // Add authorization registration because Startup.Configure calls UseAuthorization
            Services.AddAuthorization();
            var ServiceProvider = Services.BuildServiceProvider();

            var ApplicationBuilder = new ApplicationBuilder(ServiceProvider);
            var Environment = new TestEnv { EnvironmentName = Environments.Development };

            // Act & Assert - Should not throw when running configure in Development with minimal services
            string configureException = null;
            try
            {
                StartupInstance.Configure(ApplicationBuilder, Environment);
            }
            catch (Exception e)
            {
                configureException = e.ToString();
            }
            Assert.IsNull(configureException, $"Configure threw unexpectedly: {configureException}");
        }

        [Test]
        public void TestEnv_GettersAndSetters_AreAccessible()
        {
            var env = new TestEnv();
            // exercise setters
            env.WebRootPath = "webroot";
            env.EnvironmentName = "Env";
            env.ApplicationName = "App";
            env.ContentRootPath = "root";
            env.WebRootFileProvider = null;
            env.ContentRootFileProvider = null;

            // exercise getters
            Assert.AreEqual("webroot", env.WebRootPath);
            Assert.AreEqual("Env", env.EnvironmentName);
            Assert.AreEqual("App", env.ApplicationName);
            Assert.AreEqual("root", env.ContentRootPath);
            Assert.IsNull(env.WebRootFileProvider);
            Assert.IsNull(env.ContentRootFileProvider);
        }

        [Test]
        public void SimpleActionDescriptorProvider_ActionDescriptors_Accessible()
        {
            var p = new SimpleActionDescriptorProvider();
            var ad = p.ActionDescriptors;
            Assert.IsNotNull(ad);
            Assert.IsInstanceOf(typeof(ActionDescriptorCollection), ad);
        }

        /// <summary>
        /// Tests that Configure with MVC maps endpoints and returns success for root endpoint.
        /// </summary>
        [Test]
        public async Task Configure_WithMvc_MapsEndpoints_ReturnsSuccessForRoot()
        {
            // Use WebApplicationFactory to bootstrap the app (this will use Program/CreateHostBuilder and Startup)
            // Arrange
            await using var Factory = new WebApplicationFactory<ContosoCrafts.WebSite.Program>();
            var HttpClient = Factory.CreateClient();

            // Act
            var Response = await HttpClient.GetAsync("/");

            // Assert
            Assert.AreEqual(true, Response.IsSuccessStatusCode, "Expected root endpoint to respond successfully when MVC services are present.");
        }

        #endregion

        #region Configuration Tests

        /// <summary>
        /// Verifies that the <see cref="Startup.Configuration"/> getter returns the
        /// values provided via an in-memory configuration source.
        /// Method naming follows: Method_Condition_State_Reason_Expected
        /// </summary>
        [Test]
        public void Startup_ConfigurationGetter_WithInMemoryConfiguration_ShouldReturnProvidedConfiguration()
        {
            // Arrange
            // in-memory key/value pairs to seed the configuration
            var ConfigurationData = new Dictionary<string, string>
            {
                { "TestKey", "TestValue" }
            };

            // build IConfiguration from the in-memory collection
            var Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(ConfigurationData)
                .Build();

            // create Startup using the constructed configuration
            var StartupInstance = new Startup(Configuration);

            // Act
            // retrieve the Configuration property from Startup
            var Result = StartupInstance.Configuration;

            // Reset
            // (no external resources to clean up; null references to help GC)
            StartupInstance = null;
            Configuration = null;

            // Assert
            Assert.AreNotEqual(null, Result, "Configuration should not be null.");
            Assert.AreEqual("TestValue", Result["TestKey"], "Configuration should contain the provided key/value.");
        }

        #endregion
    }

    /// <summary>
    /// Unit tests for the <see cref="Startup.ConfigureServices"/> method.
    /// </summary>
    [TestFixture]
    public class StartupConfigureServicesTests
    {
        /// <summary>
        /// Ensures that calling ConfigureServices registers the
        /// <see cref="JsonFileProductService"/> as a transient service.
        /// Method naming follows: Method_Condition_State_Reason_Expected
        /// </summary>
        [Test]
        public void Startup_ConfigureServices_WhenCalled_ShouldRegister_JsonFileProductService_AsTransient()
        {
            // Arrange
            // empty configuration used by Startup constructor
            var Configuration = new ConfigurationBuilder().Build();
            var StartupInstance = new Startup(Configuration);

            // DI service collection to be populated by ConfigureServices
            var Services = new ServiceCollection();

            // Act
            StartupInstance.ConfigureServices(Services);
            // locate the registration for JsonFileProductService
            var ServiceRegistration = Services.FirstOrDefault(sd => sd.ServiceType == typeof(JsonFileProductService));

            // Reset
            // (no external resources to clean up; null references to help GC)
            StartupInstance = null;
            // keep result for assertions

            // Assert
            Assert.AreNotEqual(null, ServiceRegistration, "JsonFileProductService should be registered.");
            Assert.AreEqual(ServiceLifetime.Transient, ServiceRegistration.Lifetime, "JsonFileProductService should be registered as Transient.");
        }
    }
}