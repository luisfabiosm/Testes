namespace Domain.Core.Settings
{
    public record AppSettings
    {
        public DatabaseSettings DB { get; set; } = new();
        public JwtSettings Jwt { get; set; } = new();
        public OtlpSettings? Otlp { get; set; } = new();
        public SerilogSettings Serilog { get; set; } = new();


        public AppSettings()
        {

        }
    }
}
