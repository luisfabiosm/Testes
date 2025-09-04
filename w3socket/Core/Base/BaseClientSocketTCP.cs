using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;

namespace W3Socket.Core.Base
{
    public class BaseClientSocketTCP
    {
        #region campos de atribuicao

        protected readonly ILogger<BaseClientSocketTCP> _loggerBase;
        internal ActivitySource _Activity;

        #endregion

        public string RemoteIP { get; internal set; }
        public int Port { get; internal set; }
        public int Timeout { get; internal set; }
        public int SizeBuffer { get; internal set; }

        public BaseClientSocketTCP(string remoteIP, int port, ILogger<BaseClientSocketTCP> loggerBase)
        {
            this._loggerBase = loggerBase;
            this.Port = port;
            this.RemoteIP = remoteIP;
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
