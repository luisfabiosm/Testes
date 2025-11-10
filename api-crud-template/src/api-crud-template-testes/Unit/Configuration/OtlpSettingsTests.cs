using Domain.Core.Settings;
using FluentAssertions;

namespace api_crud_template_testes.Unit.Configuration
{
    public class OtlpSettingsTests
    {
        [Fact]
        public void OtlpSettings_DefaultConstructor_ShouldInitializeProperties()
        {
            // Act
            var otlpSettings = new OtlpSettings();

            // Assert
            otlpSettings.Endpoint.Should().BeNull();
            otlpSettings.ServiceName.Should().BeNull();
            otlpSettings.ServiceVersion.Should().BeNull();
        }

        [Fact]
        public void OtlpSettings_WithCustomValues_ShouldSetPropertiesCorrectly()
        {
            // Arrange & Act
            var otlpSettings = new OtlpSettings
            {
                Endpoint = "http://localhost:4317",
                ServiceName = "api-crud-template",
                ServiceVersion = "1.0.0"
            };

            // Assert
            otlpSettings.Endpoint.Should().Be("http://localhost:4317");
            otlpSettings.ServiceName.Should().Be("api-crud-template");
            otlpSettings.ServiceVersion.Should().Be("1.0.0");
        }

        [Theory]
        [InlineData("http://localhost:4317")]
        [InlineData("https://otel-collector.example.com:4317")]
        [InlineData("grpc://telemetry-service:4317")]
        public void OtlpSettings_Endpoint_WithValidUrls_ShouldBeSet(string endpoint)
        {
            // Arrange
            var otlpSettings = new OtlpSettings();

            // Act
            otlpSettings.Endpoint = endpoint;

            // Assert
            otlpSettings.Endpoint.Should().Be(endpoint);
        }

        [Theory]
        [InlineData("my-service")]
        [InlineData("api-crud-template")]
        [InlineData("microservice.user.api")]
        public void OtlpSettings_ServiceName_WithValidNames_ShouldBeSet(string serviceName)
        {
            // Arrange
            var otlpSettings = new OtlpSettings();

            // Act
            otlpSettings.ServiceName = serviceName;

            // Assert
            otlpSettings.ServiceName.Should().Be(serviceName);
        }

        [Theory]
        [InlineData("1.0.0")]
        [InlineData("2.1.5")]
        [InlineData("1.0.0-beta")]
        [InlineData("1.2.3-alpha.1")]
        public void OtlpSettings_ServiceVersion_WithValidVersions_ShouldBeSet(string version)
        {
            // Arrange
            var otlpSettings = new OtlpSettings();

            // Act
            otlpSettings.ServiceVersion = version;

            // Assert
            otlpSettings.ServiceVersion.Should().Be(version);
        }

        [Fact]
        public void OtlpSettings_AsRecord_ShouldSupportValueEquality()
        {
            // Arrange
            var settings1 = new OtlpSettings
            {
                Endpoint = "http://localhost:4317",
                ServiceName = "test-service",
                ServiceVersion = "1.0.0"
            };

            var settings2 = new OtlpSettings
            {
                Endpoint = "http://localhost:4317",
                ServiceName = "test-service",
                ServiceVersion = "1.0.0"
            };

            var settings3 = new OtlpSettings
            {
                Endpoint = "http://different:4317",
                ServiceName = "test-service",
                ServiceVersion = "1.0.0"
            };

            // Act & Assert
            settings1.Should().Be(settings2);
            settings1.Should().NotBe(settings3);
        }
    }

}
