using System.Diagnostics;

namespace Domain.Core.Ports.Outbound
{
    public interface IOtlpServicePort
    {
        Activity GetOTLPSource(string operation, ActivityKind kind);
        void LogInformation(string operation, string information);
        void LogError(string operation, string error);
    }
}
