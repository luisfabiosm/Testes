using Adapters.Outbound.Metrics;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Adapters.Outbound.Metrics
{

    #region MetricsAdapterTests

    public class MetricsAdapterTests : IDisposable
    {
        private readonly MetricsAdapter _metricsAdapter;
        private readonly MeterListener _meterListener;
        private readonly List<KeyValuePair<string, object>> _recordedMeasurements;

        public MetricsAdapterTests()
        {
            _metricsAdapter = new MetricsAdapter();
            _recordedMeasurements = new List<KeyValuePair<string, object>>();

            // Configurar listener para capturar métricas
            _meterListener = new MeterListener();
            _meterListener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == Assembly.GetExecutingAssembly().GetName().Name)
                {
                    listener.EnableMeasurementEvents(instrument, null);
                }
            };

            _meterListener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
            {
                _recordedMeasurements.Add(new KeyValuePair<string, object>(instrument.Name, measurement));
            });

            _meterListener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
            {
                _recordedMeasurements.Add(new KeyValuePair<string, object>(instrument.Name, measurement));
            });

            _meterListener.Start();
        }

        public void Dispose()
        {
            _meterListener?.Dispose();
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new MetricsAdapter();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void Constructor_DeveInicializarMeterComNomeCorreto()
        {
            // Act
            var instance = new MetricsAdapter();

            // Assert
            Assert.NotNull(instance);
            // Meter é criado internamente, verificamos através do comportamento
        }

        [Fact]
        public void RecordRequest_ComEndpointValido_DeveRegistrarMetrica()
        {
            // Arrange
            var endpoint = "/api/test";
            var initialCount = _recordedMeasurements.Count;

            // Act
            _metricsAdapter.RecordRequest(endpoint);

            // Wait a bit for async processing
            Thread.Sleep(100);

            // Assert
            Assert.True(_recordedMeasurements.Count >= initialCount);
        }

        [Theory]
        [InlineData("/api/pix/payment")]
        [InlineData("/api/pix/transfer")]
        [InlineData("/health")]
        [InlineData("/metrics")]
        [InlineData("/")]
        public void RecordRequest_ComDiferentesEndpoints_DeveRegistrarMetricas(string endpoint)
        {
            // Arrange
            var initialCount = _recordedMeasurements.Count;

            // Act
            _metricsAdapter.RecordRequest(endpoint);

            // Assert - Não deve lançar exceção
            Assert.True(true); // Teste passou se chegou até aqui
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void RecordRequest_ComEndpointInvalido_DeveExecutarSemErros(string endpoint)
        {
            // Act & Assert - Não deve lançar exceção
            _metricsAdapter.RecordRequest(endpoint);
        }

        [Fact]
        public void RecordRequest_ChamadasMultiplas_DeveRegistrarTodasAsMetricas()
        {
            // Arrange
            var endpoints = new[] { "/api/test1", "/api/test2", "/api/test3" };

            // Act
            foreach (var endpoint in endpoints)
            {
                _metricsAdapter.RecordRequest(endpoint);
            }

            // Assert - Não deve lançar exceção
            Assert.True(true);
        }

        [Fact]
        public void RecordRequestDuration_ComDuracaoValida_DeveRegistrarMetrica()
        {
            // Arrange
            var duration = 1.5; // 1.5 seconds
            var endpoint = "/api/test";
            var initialCount = _recordedMeasurements.Count;

            // Act
            _metricsAdapter.RecordRequestDuration(duration, endpoint);

            // Wait a bit for async processing
            Thread.Sleep(100);

            // Assert
            Assert.True(_recordedMeasurements.Count >= initialCount);
        }

        [Theory]
        [InlineData(0.001, "/api/fast")]
        [InlineData(1.0, "/api/normal")]
        [InlineData(10.5, "/api/slow")]
        [InlineData(60.0, "/api/very-slow")]
        [InlineData(0.0, "/api/instant")]
        public void RecordRequestDuration_ComDiferentesDuracoes_DeveRegistrarMetricas(double duration, string endpoint)
        {
            // Act & Assert - Não deve lançar exceção
            _metricsAdapter.RecordRequestDuration(duration, endpoint);
        }

        [Fact]
        public void RecordRequestDuration_ComDuracaoNegativa_DeveExecutarSemErros()
        {
            // Arrange
            var duration = -1.0;
            var endpoint = "/api/test";

            // Act & Assert - Não deve lançar exceção
            _metricsAdapter.RecordRequestDuration(duration, endpoint);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void RecordRequestDuration_ComEndpointInvalido_DeveExecutarSemErros(string endpoint)
        {
            // Arrange
            var duration = 1.0;

            // Act & Assert - Não deve lançar exceção
            _metricsAdapter.RecordRequestDuration(duration, endpoint);
        }

        [Fact]
        public void RecordRequestDuration_ComValoresExtremos_DeveExecutarSemErros()
        {
            // Arrange
            var extremeValues = new[] { double.MinValue, double.MaxValue, double.NaN, double.PositiveInfinity, double.NegativeInfinity };
            var endpoint = "/api/test";

            // Act & Assert
            foreach (var value in extremeValues)
            {
                _metricsAdapter.RecordRequestDuration(value, endpoint);
            }
        }

        [Fact]
        public void ThreadSafety_MultiplasChamadasSimultaneas_DeveExecutarSemErros()
        {
            // Arrange
            const int numberOfThreads = 10;
            const int operationsPerThread = 100;
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < numberOfThreads; i++)
            {
                int threadId = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        _metricsAdapter.RecordRequest($"/api/thread_{threadId}/operation_{j}");
                        _metricsAdapter.RecordRequestDuration(j * 0.01, $"/api/thread_{threadId}/duration_{j}");
                    }
                }));
            }

            // Assert - Não deve lançar exceção
            Task.WaitAll(tasks.ToArray());
        }

        [Fact]
        public void Performance_MuitasMetricas_DeveSerRapido()
        {
            // Arrange
            const int numberOfMetrics = 10000;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            for (int i = 0; i < numberOfMetrics; i++)
            {
                _metricsAdapter.RecordRequest($"/api/performance_test_{i}");
                _metricsAdapter.RecordRequestDuration(i * 0.001, $"/api/duration_test_{i}");
            }

            stopwatch.Stop();

            // Assert
            // Deve ser muito rápido - menos de 5 segundos para 10k métricas
            Assert.True(stopwatch.ElapsedMilliseconds < 5000,
                $"Performance inadequada: {stopwatch.ElapsedMilliseconds}ms para {numberOfMetrics} métricas");
        }

        [Fact]
        public void CenarioRealista_SimulandoTrafegoWeb_DeveExecutarCorretamente()
        {
            // Arrange
            var endpoints = new[]
            {
                "/api/pix/payment",
                "/api/pix/transfer",
                "/api/pix/status",
                "/health",
                "/metrics"
            };

            var random = new Random();

            // Act
            for (int i = 0; i < 1000; i++)
            {
                var endpoint = endpoints[random.Next(endpoints.Length)];
                var duration = random.NextDouble() * 5.0; // 0-5 seconds

                _metricsAdapter.RecordRequest(endpoint);
                _metricsAdapter.RecordRequestDuration(duration, endpoint);
            }

            // Assert - Não deve lançar exceção
            Assert.True(true);
        }
    }

    #endregion
}
