namespace Domain.Core.Models.Settings
{
    public record GCSrvSettings
    {
        public bool Ativo { get; set; }
        public int TimerColetar { get; set; }
        public int MemoryTrigger { get; set; }
        public int AddHoursToUTC { get; set; } 
        public GCSrvSettings() { }
    }
}
