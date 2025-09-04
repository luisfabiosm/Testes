using W3Socket.Core.Models.SPA;

namespace W3Socket.Core.Interfaces
{
    public delegate void DelReceberMensagem(SpaTcpBufferArgs args);

    public interface ISPAClientSocketTCP
    {
        void ConnectHost(int ConnectTimeOut = 10, int SendTimeout = 10, int ReceiveBufferSize = 1024);
        void SendAsyncData(byte[] bytMessage);
        bool IsConnected();

        event DelReceberMensagem OnDataArrival;
    }
}
