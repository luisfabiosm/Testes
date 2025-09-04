using Domain.Core.Ports.Inbound;
using System.Threading.Channels;

namespace Adapters.Inbound.ProcessadorFilaAdapter.Processador
{
    public class BackgroundTaskQueue : IBackgroundTaskQueuePort
    {
        private readonly Channel<Func<CancellationToken, ValueTask>> _queue;

        public BackgroundTaskQueue()
        {
            _queue = Channel.CreateUnbounded<Func<CancellationToken, ValueTask>>();
        }

        public ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem)
        {
            Console.WriteLine($"Total fila : {_queue.Reader.Count}");
            return _queue.Writer.WriteAsync(workItem);
        }
    }
}
