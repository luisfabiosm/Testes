namespace Domain.Core.Settings
{
    public record AppSettings
    {
        public DBSettings DB { get; set; }

        public SPASettings SPA { get; set; }

        public OtlpSettings Otlp { get; set; }

        public GCSrvSettings GC { get; set; }

        public AppSettings()
        {

        }
    }
}
