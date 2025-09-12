using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Data;

namespace pix_pagador_testes.TestUtilities.Fixtures;
public class ControllerFixture : IDisposable
{
    public WebApplicationFactory<Program> Factory { get; private set; }
    public HttpClient Client { get; private set; }
    public IServiceProvider Services => Factory.Services;

    public ControllerFixture()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=TestDB;Integrated Security=true;",
                        ["DatabaseSettings:CommandTimeout"] = "10",
                        ["DatabaseSettings:RetryCount"] = "1"
                    });
                });

                builder.ConfigureTestServices(services =>
                {
                    // Override services for testing
                    // Remove real database dependencies
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IDbConnection));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add mock services
                    services.AddSingleton<Mock<IDbConnection>>();

                    // Configure logging for tests
                    services.AddLogging(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Warning);
                        builder.AddConsole();
                    });
                });
            });

        Client = Factory.CreateClient();
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
