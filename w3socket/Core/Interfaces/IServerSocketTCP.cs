using W3Socket.Core.Models.SPA;
using System.Threading.Tasks;
using System;

namespace W3Socket.Core.Interfaces
{
    public interface IServerSocketTCP
    {
        Task StartListening();

        event Func<TcpServerBufferArgs, Task> OnDataArrival;
        bool IsListenning();
        string GetServerIP();
        int GetServerPort();
    }
}
