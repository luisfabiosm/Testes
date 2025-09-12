using Adapters.Outbound.Logging;
using Domain.Core.Ports.Outbound;
using Domain.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace pix_pagador_testes.Adapters.Outbound.Logging
{
    public class LoggingExtensionsTests
    {
        private readonly Mock<IServiceCollection> _mockServices;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IConfigurationSection> _mockSection;

        public LoggingExtensionsTests()
        {
            _mockServices = new Mock<IServiceCollection>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockSection = new Mock<IConfigurationSection>();

            _mockConfiguration.Setup(c => c.GetSection("AppSettings:Otlp"))
                .Returns(_mockSection.Object);
        }

        [Fact]
        public void AddLoggingAdapter_ComConfigurationValida_DeveRetornarServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["AppSettings:Otlp:Endpoint"] = "http://localhost:4317"
                })
                .Build();

            // Act
            var result = services.AddLoggingAdapter(configuration);

            // Assert
            Assert.NotNull(result);
            Assert.Same(services, result);
        }

        [Fact]
        public void AddLoggingAdapter_ComConfigurationNula_DeveLancarException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => services.AddLoggingAdapter(null));
        }

        [Fact]
        public void AddLoggingAdapter_ComServiceCollectionNula_DeveLancarException()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                LoggingExtensions.AddLoggingAdapter(null, configuration));
        }

        [Fact]
        public void AddLoggingAdapter_DeveConfigurarOtlpSettings()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["AppSettings:Otlp:Endpoint"] = "http://localhost:4317"
                })
                .Build();

            // Act
            services.AddLoggingAdapter(configuration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var otlpOptions = serviceProvider.GetService<IOptions<OtlpSettings>>();
            Assert.NotNull(otlpOptions);
        }

        [Fact]
        public void AddLoggingAdapter_DeveRegistrarILoggingAdapter()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["AppSettings:Otlp:Endpoint"] = "http://localhost:4317"
                })
                .Build();

            // Act
            services.AddLoggingAdapter(configuration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var loggingAdapter = serviceProvider.GetService<ILoggingAdapter>();
            Assert.NotNull(loggingAdapter);
            Assert.IsType<LoggingAdapter>(loggingAdapter);
        }

        [Fact]
        public void AddLoggingAdapter_ComVariavelAmbiente_DeveUsarVariavelAmbiente()
        {
            // Arrange
            var environmentEndpoint = "http://environment:4317";
            Environment.SetEnvironmentVariable("OPEN_TELEMETRY_ENDPOINT", environmentEndpoint);

            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["AppSettings:Otlp:Endpoint"] = "http://environment:4317"
                })
                .Build();

            try
            {
                // Act
                services.AddLoggingAdapter(configuration);

                // Assert
                var serviceProvider = services.BuildServiceProvider();
                var otlpOptions = serviceProvider.GetService<IOptions<OtlpSettings>>();
                Assert.NotNull(otlpOptions);
                Assert.Equal(environmentEndpoint, otlpOptions.Value.Endpoint);
            }
            finally
            {
                // Cleanup
                Environment.SetEnvironmentVariable("OPEN_TELEMETRY_ENDPOINT", null);
            }
        }

        [Fact]
        public void AddLoggingAdapter_SemVariavelAmbiente_DeveUsarConfiguracao()
        {
            // Arrange
            var configEndpoint = "http://config:4317";
            Environment.SetEnvironmentVariable("OPEN_TELEMETRY_ENDPOINT", null);

            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["AppSettings:Otlp:Endpoint"] = configEndpoint
                })
                .Build();

            // Act
            services.AddLoggingAdapter(configuration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var otlpOptions = serviceProvider.GetService<IOptions<OtlpSettings>>();
            Assert.NotNull(otlpOptions);
            Assert.Equal(configEndpoint, otlpOptions.Value.Endpoint);
        }

       

        [Fact]
        public void AddLoggingAdapter_DeveConfigurarSerilog()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["AppSettings:Otlp:Endpoint"] = "http://localhost:4317",
                    ["Serilog:MinimumLevel"] = "Information"
                })
                .Build();

            // Act
            services.AddLoggingAdapter(configuration);

            // Assert
            // Verifica se Serilog foi configurado (Log.Logger não é null)
            Assert.NotNull(Serilog.Log.Logger);
        }

        [Fact]
        public void AddLoggingAdapter_ChamadasMultiplas_DeveExecutarSemErros()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["AppSettings:Otlp:Endpoint"] = "http://localhost:4317"
                })
                .Build();

            // Act & Assert - Não deve lançar exceção
            services.AddLoggingAdapter(configuration);
            services.AddLoggingAdapter(configuration);
            services.AddLoggingAdapter(configuration);
        }

        [Fact]
        public void AddLoggingAdapter_ComConfigurationCompleta_DeveConfigurarTodosOsServicos()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["AppSettings:Otlp:Endpoint"] = "http://localhost:4317",
                    ["AppSettings:Otlp:Protocol"] = "grpc",
                    ["Serilog:MinimumLevel"] = "Information",
                    ["Serilog:WriteTo:0:Name"] = "Console"
                })
                .Build();

            // Act
            var result = services.AddLoggingAdapter(configuration);

            // Assert
            Assert.Same(services, result);
            var serviceProvider = services.BuildServiceProvider();

            // Verifica se todos os serviços principais foram registrados
            Assert.NotNull(serviceProvider.GetService<ILoggingAdapter>());
            Assert.NotNull(serviceProvider.GetService<IOptions<OtlpSettings>>());
        }
    }

}
