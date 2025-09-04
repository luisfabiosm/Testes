using W3Socket.Core.Models.SPA;

namespace W3Socket.Core.Tasks
{
    public delegate void DataProcessCallback(SpaTcpBufferArgs args);

    public class BackgroundTask
    {
        private readonly SpaTcpBufferArgs TCPArgs;
        private readonly DataProcessCallback ReturnCallback;

        public BackgroundTask(SpaTcpBufferArgs args, DataProcessCallback callback)
        {
            TCPArgs = args;
            ReturnCallback = callback;
        }

        public void ThreadProcessed()
        {
            if (ReturnCallback != null)
                ReturnCallback(TCPArgs);
        }
    }
}
