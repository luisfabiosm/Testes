using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace W3Socket.Core.Base
{
    public class BaseServerSocketTCP
    {
        #region Campos de atribuicao

        protected readonly ILogger<BaseServerSocketTCP> _loggerBase;
        internal ActivitySource _Activity;
        internal ManualResetEvent _allDone = new ManualResetEvent(false);
        internal System.Timers.Timer _tmrThreadClient;
        internal ConcurrentDictionary<string, Socket> _clients = new ConcurrentDictionary<string, Socket>();
        internal TcpListener _server;
        internal bool _listenning = false;

        #endregion

        public IPAddress IP { get; internal set; }
        public int Port { get; internal set; }
        public int Timeout { get; internal set; }
        public int SizeBuffer { get; internal set; }

        public BaseServerSocketTCP(int port, ILogger<BaseServerSocketTCP> loggerBase)
        {
            try
            {
                this._loggerBase = loggerBase;
                this.Port = port;
                this.IP = IPAddress.Any;
            }
            catch
            {
                throw;
            }
        }

        public void LogInformation(string operation, string information)
        {
            using (var _activity = _Activity.StartActivity($"Information: {operation}"))
            {
                _activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Ok);
                string threadId = Thread.CurrentThread.ManagedThreadId.ToString("D6");
                string threadName = Thread.CurrentThread.Name ?? "Unknown";
                _activity?.AddEvent(new ActivityEvent($"[{threadName}][{threadId}] {information}"));
            }
        }

        public void LogError(string operation, string error)
        {
            using (var _activity = _Activity.StartActivity($"Error: {operation}"))
            {
                string threadId = Thread.CurrentThread.ManagedThreadId.ToString("D6");
                string threadName = Thread.CurrentThread.Name ?? "Unknown";
                _activity?.SetStatus(ActivityStatusCode.Error, $"[{threadId}] {error}");
                _activity?.AddEvent(new ActivityEvent($"[{threadName}][{threadId}] {error}"));
            }
        }
    }
}
