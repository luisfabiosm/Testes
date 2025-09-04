using Domain.Core.Models.Settings;
using Domain.Core.Ports.Outbound;
using System.Diagnostics;
using System.Reflection;

namespace Adapters.Outbound.OtlpAdapter
{
    public class OtlpService : IOtlpServicePort
    {
        #region variáveis

        private readonly OtlpSettings _settings;
        private readonly ActivitySource _activitySource;

        #endregion

        public OtlpService(OtlpSettings settings)
        {
            _settings = settings;
            _settings.ServiceName = Assembly.GetExecutingAssembly().GetName().Name!;
            _activitySource = new ActivitySource(_settings.ServiceName);
        }

        public Activity GetOTLPSource(string operation, ActivityKind kind)
        {
            return _activitySource?.StartActivity(operation, kind)!;
        }

        public void LogInformation(string operation, string information)
        {
            using (var _activity = OtlpActivityService.GenerateActivitySource.StartActivity($"Information: {operation}", ActivityKind.Internal))
            {
                _activity?.SetStatus(ActivityStatusCode.Ok);

                string threadId = Thread.CurrentThread.ManagedThreadId.ToString("D6");
                string threadName = Thread.CurrentThread.Name ?? "Unknown";
                _activity?.AddEvent(new ActivityEvent($"[{threadName}][{threadId}] {information}"));
            }
        }

        public void LogError(string operation, string error)
        {
            using (var _activity = OtlpActivityService.GenerateActivitySource.StartActivity($"Error: {operation}", ActivityKind.Internal))
            { 
                string threadId = Thread.CurrentThread.ManagedThreadId.ToString("D6");
                string threadName = Thread.CurrentThread.Name ?? "Unknown";
                _activity?.SetStatus(ActivityStatusCode.Error, $"[{threadId}] {error}");
                _activity?.AddEvent(new ActivityEvent($"[{threadName}][{threadId}] {error}"));
            }
        }
    }

    public static class OtlpActivityService
    {
        private static readonly string ServiceName = Assembly.GetExecutingAssembly().GetName().Name!;
        private static readonly string ServiceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        public static readonly ActivitySource GenerateActivitySource = new(ServiceName, ServiceVersion);

        // Método auxiliar para debug
        public static bool IsEnabled()
        {
            var hasListeners = GenerateActivitySource.HasListeners();
            Console.WriteLine($"ActivitySource '{ServiceName}' has listeners: {hasListeners}");
            return hasListeners;
        }
    }
}
