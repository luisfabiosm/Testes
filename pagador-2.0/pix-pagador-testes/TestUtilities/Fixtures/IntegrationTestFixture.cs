using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;


namespace pix_pagador_testes.TestUtilities.Fixtures;

public class IntegrationTestFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program> Factory { get; private set; }
    public HttpClient Client { get; private set; }
    public DatabaseFixture DatabaseFixture { get; private set; }

    public IntegrationTestFixture()
    {
        DatabaseFixture = new DatabaseFixture();
    }

    public async Task InitializeAsync()
    {
        await DatabaseFixture.InitializeAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Integration");

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = DatabaseFixture.ConnectionString,
                        ["DatabaseSettings:CommandTimeout"] = "30",
                        ["DatabaseSettings:RetryCount"] = "3"
                    });
                });

                builder.ConfigureTestServices(services =>
                {
                    // Configure for integration testing
                    services.AddLogging(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Information);
                        builder.AddConsole();
                    });
                });
            });

        Client = Factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        Factory?.Dispose();
        await DatabaseFixture.DisposeAsync();
    }
}

