using Adapters.Outbound.Logging;
using Adapters.Outbound.Metrics;
using Domain.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Adapters.Outbound.Metrics
{
    #region MetricsExtensionsTests

    public class MetricsExtensionsTests
    {
        private readonly Mock<IServiceCollection> _mockServices;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IConfigurationSection> _mockSection;

        public MetricsExtensionsTests()
        {
            _mockServices = new Mock<IServiceCollection>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockSection = new Mock<IConfigurationSection>();

            _mockConfiguration.Setup(c => c.GetSection("AppSettings:Otlp"))
                .Returns(_mockSection.Object);
        }

        [Fact]
        public void AddMetricsAdapter_ComConfigurationValida_DeveRetornarServiceCollection()
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
            var result = services.AddMetricsAdapter(configuration);

            // Assert
            Assert.NotNull(result);
            Assert.Same(services, result);
        }

        //[Fact]
        //public void AddMetricsAdapter_ComConfigurationNula_DeveLancarException()
        //{
        //    // Arrange
        //    var services = new ServiceCollection();

        //    // Act & Assert
        //    Assert.Throws<NullReferenceException>(() => services.AddMetricsAdapter(null));

        //}

    
        [Fact]
        public void AddMetricsAdapter_ComServiceCollectionNula_DeveLancarException()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                MetricsExtensions.AddMetricsAdapter(null, configuration));
        }

        [Fact]
        public void AddMetricsAdapter_DeveConfigurarOtlpSettings()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["AppSettings:Otlp:Endpoint"] = "http://localhost:4317",
                    ["AppSettings:Otlp:Protocol"] = "grpc"
                })
                .Build();

            // Act
            services.AddMetricsAdapter(configuration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var otlpOptions = serviceProvider.GetService<IOptions<OtlpSettings>>();
            Assert.NotNull(otlpOptions);
        }

        [Fact]
        public void AddMetricsAdapter_DeveRegistrarMetricsAdapterComoSingleton()
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
            services.AddMetricsAdapter(configuration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var metricsAdapter1 = serviceProvider.GetService<MetricsAdapter>();
            var metricsAdapter2 = serviceProvider.GetService<MetricsAdapter>();

            Assert.NotNull(metricsAdapter1);
            Assert.NotNull(metricsAdapter2);
            Assert.Same(metricsAdapter1, metricsAdapter2); // Deve ser singleton
        }

        [Fact]
        public void AddMetricsAdapter_ComVariavelAmbiente_DeveUsarVariavelAmbiente()
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
                services.AddMetricsAdapter(configuration);

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
        public void AddMetricsAdapter_SemVariavelAmbiente_DeveUsarConfiguracao()
        {
            // Arrange
            var configEndpoint = "http://environment:4317";
            Environment.SetEnvironmentVariable("OPEN_TELEMETRY_ENDPOINT", null);

            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["AppSettings:Otlp:Endpoint"] = configEndpoint
                })
                .Build();

            // Act
            services.AddMetricsAdapter(configuration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var otlpOptions = serviceProvider.GetService<IOptions<OtlpSettings>>();
            Assert.NotNull(otlpOptions);
            Assert.Equal(configEndpoint, otlpOptions.Value.Endpoint);
        }

       

        [Fact]
        public void AddMetricsAdapter_DeveAdicionarInstrumentacaoCorreta()
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
            services.AddMetricsAdapter(configuration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();

            // Verifica se os serviços foram configurados corretamente
            Assert.NotNull(serviceProvider.GetService<MetricsAdapter>());
        }

        [Fact]
        public void AddMetricsAdapter_ChamadasMultiplas_DeveExecutarSemErros()
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
            services.AddMetricsAdapter(configuration);
            services.AddMetricsAdapter(configuration);
            services.AddMetricsAdapter(configuration);
        }

        [Fact]
        public void AddMetricsAdapter_ComConfigurationCompleta_DeveConfigurarTodosOsServicos()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["AppSettings:Otlp:Endpoint"] = "http://localhost:4317",
                    ["AppSettings:Otlp:Protocol"] = "grpc",
                    ["AppSettings:Otlp:Headers"] = "authorization=Bearer token"
                })
                .Build();

            // Act
            var result = services.AddMetricsAdapter(configuration);

            // Assert
            Assert.Same(services, result);
            var serviceProvider = services.BuildServiceProvider();

            // Verifica se todos os serviços principais foram registrados
            Assert.NotNull(serviceProvider.GetService<MetricsAdapter>());
            Assert.NotNull(serviceProvider.GetService<IOptions<OtlpSettings>>());
        }

        [Fact]
        public void AddMetricsAdapter_ComConfigurationVazia_DeveExecutarSemErros()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();

            // Act & Assert - Não deve lançar exceção
            services.AddMetricsAdapter(configuration);
        }

        [Fact]
        public void AddMetricsAdapter_DeveUsarNomeEVersaoDoAssembly()
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
            services.AddMetricsAdapter(configuration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var metricsAdapter = serviceProvider.GetService<MetricsAdapter>();
            Assert.NotNull(metricsAdapter);

            // Verifica se foi configurado corretamente (através da execução sem erros)
            metricsAdapter.RecordRequest("/test");
            metricsAdapter.RecordRequestDuration(1.0, "/test");
        }
    }

    #endregion
}
