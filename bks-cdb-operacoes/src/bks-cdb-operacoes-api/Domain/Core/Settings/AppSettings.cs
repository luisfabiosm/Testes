namespace Domain.Core.Settings
{
    public record AppSettings
    {
        public OtlpSettings Otlp { get; set; }
        public CDBSettings CDB { get; set; }

   
        public AppSettings()
        {

        }
    }
}
