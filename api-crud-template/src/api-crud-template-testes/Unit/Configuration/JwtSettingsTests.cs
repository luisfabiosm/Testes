using Domain.Core.Settings;
using FluentAssertions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_crud_template_testes.Unit.Configuration
{
    public class JwtSettingsTests
    {
        [Fact]
        public void JwtSettings_DefaultConstructor_ShouldSetDefaultValues()
        {
            // Act
            var jwtSettings = new JwtSettings();

            // Assert
            jwtSettings.Key.Should().Be(string.Empty);
            jwtSettings.Issuer.Should().Be(string.Empty);
            jwtSettings.Audience.Should().Be(string.Empty);
            jwtSettings.ExpirationMinutes.Should().Be(60);
        }

        [Fact]
        public void JwtSettings_WithCustomValues_ShouldSetPropertiesCorrectly()
        {
            // Arrange & Act
            var jwtSettings = new JwtSettings
            {
                Key = "super-secret-key-for-jwt-signing",
                Issuer = "test-issuer",
                Audience = "test-audience",
                ExpirationMinutes = 120
            };

            // Assert
            jwtSettings.Key.Should().Be("super-secret-key-for-jwt-signing");
            jwtSettings.Issuer.Should().Be("test-issuer");
            jwtSettings.Audience.Should().Be("test-audience");
            jwtSettings.ExpirationMinutes.Should().Be(120);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(15)]
        [InlineData(60)]
        [InlineData(1440)] // 24 hours
        [InlineData(10080)] // 7 days
        public void JwtSettings_ExpirationMinutes_WithValidValues_ShouldBeSet(int minutes)
        {
            // Arrange
            var jwtSettings = new JwtSettings();

            // Act
            jwtSettings.ExpirationMinutes = minutes;

            // Assert
            jwtSettings.ExpirationMinutes.Should().Be(minutes);
        }

        [Theory]
        [InlineData("")]
        [InlineData("short")]
        [InlineData("a-very-long-key-that-should-be-acceptable-for-jwt-signing-purposes")]
        public void JwtSettings_Key_WithVariousLengths_ShouldBeSet(string key)
        {
            // Arrange
            var jwtSettings = new JwtSettings();

            // Act
            jwtSettings.Key = key;

            // Assert
            jwtSettings.Key.Should().Be(key);
        }

        [Fact]
        public void JwtSettings_AsRecord_ShouldSupportValueEquality()
        {
            // Arrange
            var settings1 = new JwtSettings
            {
                Key = "test-key",
                Issuer = "test-issuer",
                Audience = "test-audience",
                ExpirationMinutes = 60
            };

            var settings2 = new JwtSettings
            {
                Key = "test-key",
                Issuer = "test-issuer",
                Audience = "test-audience",
                ExpirationMinutes = 60
            };

            var settings3 = new JwtSettings
            {
                Key = "different-key",
                Issuer = "test-issuer",
                Audience = "test-audience",
                ExpirationMinutes = 60
            };

            // Act & Assert
            settings1.Should().Be(settings2);
            settings1.Should().NotBe(settings3);
        }
    }

}
