using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace pix_pagador_testes.TestUtilities.Fixtures;

public static class TestEnvironment
{
    public static void SetupTestLogging(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Warning);
            // Filtros específicos para testes
            builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
            builder.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
            builder.AddFilter("System.Net.Http", LogLevel.Warning);
        });
    }

    public static IConfiguration CreateTestConfiguration(Dictionary<string, string>? additionalSettings = null)
    {
        var baseSettings = new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=TestDB;Integrated Security=true;",
            ["DatabaseSettings:CommandTimeout"] = "30",
            ["DatabaseSettings:RetryCount"] = "3",
            ["Logging:LogLevel:Default"] = "Warning"
        };

        if (additionalSettings != null)
        {
            foreach (var setting in additionalSettings)
            {
                baseSettings[setting.Key] = setting.Value;
            }
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(baseSettings)
            .Build();
    }

    public static Mock<T> CreateMockWithLogging<T>() where T : class
    {
        var mock = new Mock<T>();

        // Para ILogger<T>, não é necessário fazer setup do método Log
        // O mock funciona adequadamente sem configuração específica
        // Se precisar verificar se logs foram chamados, use extensões específicas

        return mock;
    }

    // Método auxiliar específico para criar mocks de ILogger se necessário
    public static Mock<ILogger<T>> CreateLoggerMock<T>()
    {
        return new Mock<ILogger<T>>();
    }
}

[CollectionDefinition("Database Collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // Esta classe não tem código, é apenas para definir a collection
}

[CollectionDefinition("Integration Collection")]
public class IntegrationCollection : ICollectionFixture<IntegrationTestFixture>
{
    // Esta classe não tem código, é apenas para definir a collection
}