namespace Domain.Core.Ports.Inbound
{
    public interface IBackgroundTaskQueuePort
    {
        ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);
    }
}
