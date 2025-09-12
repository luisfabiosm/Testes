using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace pix_pagador_testes.TestUtilities.Fixtures;
public class ServiceFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; private set; }
    public IConfiguration Configuration { get; private set; }
    private readonly ServiceCollection _services;

    public ServiceFixture()
    {
        _services = new ServiceCollection();
        Configuration = BuildConfiguration();

        ConfigureServices(_services);
        ServiceProvider = _services.BuildServiceProvider();
    }

    private IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=TestDB;Integrated Security=true;",
                ["DatabaseSettings:CommandTimeout"] = "30",
                ["DatabaseSettings:RetryCount"] = "3",
                ["Logging:LogLevel:Default"] = "Warning",
                ["Logging:LogLevel:Tests"] = "Information"
            })
            .Build();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        // Configuration
        services.AddSingleton(Configuration);

        // AutoFixture for test data generation
        services.AddSingleton<IFixture>(new Fixture());

        // Add any additional test services here
    }

    public T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
