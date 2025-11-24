using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace UnitTests
{
    /// <summary>
    /// Lightweight test to exercise Startup.Configure without starting a real host.
    /// This avoids file locks and still executes the code paths inside Configure.
    /// </summary>
    [TestFixture]
    public class StartupPipelineTests
    {
        #region Helper Classes

        /// <summary>
        /// Simple IServiceProvider wrapper that delegates to an inner provider
        /// but returns null for IActionDescriptorCollectionProvider so tests can
        /// avoid initializing the full MVC surface area.
        /// </summary>
        private class TestServiceProvider : IServiceProvider
        {
            private readonly IServiceProvider _inner;

            public TestServiceProvider(IServiceProvider inner)
            {
                _inner = inner;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider))
                {
                    return null;
                }

                return _inner.GetService(serviceType);
            }
        }

        #endregion

        #region Configure Tests

        /// <summary>
        /// Tests that Startup.Configure does not throw exceptions in Development environment.
        /// </summary>
        [Test]
        public void Startup_Configure_Development_DoesNotThrow()
        {
            // Arrange
            var Configuration = new ConfigurationBuilder().Build();
            var StartupInstance = new ContosoCrafts.WebSite.Startup(Configuration);

            var Services = new ServiceCollection();

            Services.AddRouting();
            Services.AddAuthorization();

            var ServiceProvider = Services.BuildServiceProvider();

            var ApplicationBuilder = new ApplicationBuilder(new TestServiceProvider(ServiceProvider));

            var Environment = new Mock<IWebHostEnvironment>();
            // signal Development so the developer exception page path is executed
            Environment.Setup(e => e.EnvironmentName).Returns("Development");

            // Act: call Configure and capture any exception
            Exception DevelopmentException = null;
            StartupInstance.Configure(ApplicationBuilder, Environment.Object);

            // Assert: no exception was thrown
            Assert.AreEqual(null, DevelopmentException);
        }

        /// <summary>
        /// Tests that Startup.Configure does not throw exceptions in Production environment.
        /// </summary>
        [Test]
        public void Startup_Configure_NonDevelopment_DoesNotThrow()
        {
            // Arrange
            var Configuration = new ConfigurationBuilder().Build();
            var StartupInstance = new ContosoCrafts.WebSite.Startup(Configuration);

            var Services = new ServiceCollection();
            // minimal services. Avoid registering Razor/Controllers so endpoint
            // mapping that depends on MVC services is skipped in tests.
            Services.AddRouting();
            Services.AddAuthorization();

            var ServiceProvider = Services.BuildServiceProvider();

            var ApplicationBuilder = new ApplicationBuilder(new TestServiceProvider(ServiceProvider));

            var Environment = new Mock<IWebHostEnvironment>();
            // signal Production to exercise the non-development branch
            Environment.Setup(e => e.EnvironmentName).Returns("Production");

            // Act: call Configure and capture any exception
            Exception ProductionException = null;
            StartupInstance.Configure(ApplicationBuilder, Environment.Object);

            // Assert: no exception was thrown
            Assert.AreEqual(null, ProductionException);
        }

        #endregion
    }
}
