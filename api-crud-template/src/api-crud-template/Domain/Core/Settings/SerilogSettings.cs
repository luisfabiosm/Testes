namespace Domain.Core.Settings
{

    public record SerilogSettings
    {
        public MinimumLevelSettings MinimumLevel { get; set; } = new();
    }
}
