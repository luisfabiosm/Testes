using Domain.Core.Ports.Outbound;
using System.Diagnostics;

namespace Domain.Core.Base
{
    public class BaseBackgroundService : BackgroundService
    {
        protected readonly IOtlpServicePort _otlpSource;

        public BaseBackgroundService(IServiceProvider serviceProvider)
        {
            _otlpSource = serviceProvider.GetRequiredService<IOtlpServicePort>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }

        public void LogInformation(string operation, string information)
        {
            _otlpSource.LogInformation(operation, information);
        }

        public void LogError(string operation, Exception ex)
        {
            using (var _activity = _otlpSource.GetOTLPSource($"ERRO", ActivityKind.Internal))
            {
                _activity?.SetStatus(ActivityStatusCode.Error);
                _activity?.SetTag("OrigemErro", operation);
                _activity?.SetTag("MensagemErro", ex.Message);
                _activity?.SetTag("Stack", ex.StackTrace);
            }
        }
    }
}
