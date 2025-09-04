using W3Socket.Core.Models.SPA;

namespace W3Socket.Core.Interfaces
{

    public interface IClientSocketTCP
    {

        void ConnectHost(int timeOut = 30);


        byte[] SendSyncData(byte[] bytMessage, int timeOut, int sizeBuffer);


        void SendAsyncData(byte[] bytMessage, int timeOut, int sizeBuffer);


        void OnClientDataArrival(SpaTcpBufferArgs e);

        bool IsConnected();

        string GetEndpoint();

    }
}
