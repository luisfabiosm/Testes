using Microsoft.Extensions.Options;
using Domain.Core.Models.Settings;
using Domain.Core.Ports.Outbound;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Reflection;

namespace Adapters.Outbound.OtlpAdapter.Configuration
{
    public static class OtlpConfiguration
    {
        public static IServiceCollection AddOtlpAdapter(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<OtlpSettings>(options =>
            {
                var section = configuration.GetSection("AppSettings:Otlp");
                section.Bind(options);

                // Override specific values from environment variables or constants
                options.Endpoint = Environment.GetEnvironmentVariable("OPEN_TELEMETRY_ENDPOINT") ?? options.Endpoint;
                options.ServiceName = Assembly.GetExecutingAssembly().GetName().Name ?? "";
                options.ServiceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

                Console.WriteLine($"OPEN_TELEMETRY_ENDPOINT: {options.Endpoint}");
            });

            services.AddSingleton<IOtlpServicePort>(provider =>
            {
                var settings = provider.GetRequiredService<IOptions<OtlpSettings>>().Value;
                return new OtlpService(settings);
            });

            // Create ResourceBuilder for OpenTelemetry
            var _resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(
                    serviceName: Assembly.GetExecutingAssembly().GetName().Name ?? "",
                    serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0");

            // Configure OpenTelemetry for Tracing and Metrics
            services.AddOpenTelemetry()
                .WithTracing(tracing =>
                {
                    tracing
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(Environment.GetEnvironmentVariable("OPEN_TELEMETRY_ENDPOINT")!);
                        })
                       .AddConsoleExporter()
                       .AddSource(Assembly.GetExecutingAssembly().GetName().Name!)
                       .SetResourceBuilder(_resourceBuilder)
                       .SetSampler(new TraceIdRatioBasedSampler(0.1))
                       .AddAspNetCoreInstrumentation();
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .SetResourceBuilder(_resourceBuilder)
                        .AddAspNetCoreInstrumentation();
                });

            return services;
        }
    }
}
