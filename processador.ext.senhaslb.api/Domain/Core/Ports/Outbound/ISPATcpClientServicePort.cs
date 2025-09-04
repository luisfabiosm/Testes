using W3Socket.Core.Models.SPA;

namespace Domain.Core.Ports.Outbound
{
    public interface ISPATcpClientServicePort
    {
        void ConnectHost();
        Task SendResponse(tSPACabecalho msgCab, byte[] bufMessage, int timeout = 30);
        string GetEndpoint();
    }
}
