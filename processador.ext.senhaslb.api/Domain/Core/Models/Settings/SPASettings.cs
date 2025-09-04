using Domain.Core.Models.SPA;

namespace Domain.Core.Models.Settings
{
    public record SPASettings
    {
        public SPADependencia? Dependencia { get; set; }
        public string? RouterIP { get; set; }
        public int RouterPort { get; set; }
        public string? Operador { get; set; }
        public int ConnectTimeOut { get; set; }
        public int SendTimeout { get; set; }
        public int ReceiveBufferSize { get; set; }
        public bool WithQueue { get; set; }
        public int QueueDelay { get; set; }
        public SPASettings() { }
    }
}