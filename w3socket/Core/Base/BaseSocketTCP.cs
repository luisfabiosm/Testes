using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System;

namespace W3Socket.Core.Base
{
    public abstract class BaseSocketTcp
    {
        #region Campos de atribuicao

        protected readonly ILogger<BaseSocketTcp> _loggerBase;
        internal ActivitySource _Activity;
        protected Exception _lastException;
        protected CancellationTokenSource cts = new CancellationTokenSource();

        #endregion

        protected object _lock { get; set; } = new object();
        public int SizeBuffer { get; internal set; }

        protected BaseSocketTcp(IServiceProvider serviceProvider, int sizebuffer)
        {
            this.SizeBuffer = sizebuffer;
            _loggerBase = serviceProvider.GetRequiredService<ILogger<BaseSocketTcp>>(); ;
        }

        public void SetSizeBuffer(int sizebuffer)
        {
            this.SizeBuffer = sizebuffer; ;
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
