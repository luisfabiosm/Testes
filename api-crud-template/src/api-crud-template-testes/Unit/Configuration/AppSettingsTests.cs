using Domain.Core.Settings;
using FluentAssertions;
using Xunit;

namespace api_crud_template_testes.Unit.Configuration;

public class AppSettingsTests
{
    [Fact]
    public void AppSettings_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var appSettings = new AppSettings();

        // Assert
        appSettings.DB.Should().NotBeNull();
        appSettings.Jwt.Should().NotBeNull();
        appSettings.Otlp.Should().NotBeNull();
        appSettings.Serilog.Should().NotBeNull();
    }

    [Fact]
    public void AppSettings_WithCustomValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var dbSettings = new DatabaseSettings { Cluster = "test-cluster" };
        var jwtSettings = new JwtSettings { Key = "test-key" };
        var otlpSettings = new OtlpSettings { ServiceName = "test-service" };
        var serilogSettings = new SerilogSettings();

        // Act
        var appSettings = new AppSettings
        {
            DB = dbSettings,
            Jwt = jwtSettings,
            Otlp = otlpSettings,
            Serilog = serilogSettings
        };

        // Assert
        appSettings.DB.Should().Be(dbSettings);
        appSettings.Jwt.Should().Be(jwtSettings);
        appSettings.Otlp.Should().Be(otlpSettings);
        appSettings.Serilog.Should().Be(serilogSettings);
        appSettings.DB.Cluster.Should().Be("test-cluster");
        appSettings.Jwt.Key.Should().Be("test-key");
        appSettings.Otlp.ServiceName.Should().Be("test-service");
    }

    [Fact]
    public void AppSettings_AsRecord_ShouldSupportValueEquality()
    {
        // Arrange
        var settings1 = new AppSettings
        {
            DB = new DatabaseSettings { Cluster = "test" },
            Jwt = new JwtSettings { Key = "key" }
        };

        var settings2 = new AppSettings
        {
            DB = new DatabaseSettings { Cluster = "test" },
            Jwt = new JwtSettings { Key = "key" }
        };

        var settings3 = new AppSettings
        {
            DB = new DatabaseSettings { Cluster = "different" },
            Jwt = new JwtSettings { Key = "key" }
        };

        // Act & Assert
        settings1.Should().Be(settings2);
        settings1.Should().NotBe(settings3);
    }
}



