namespace Domain.Core.Settings
{
    public record CDBSettings
    {
        public bool UseSps { get; set; }
        public string Endpoint { get; set; }


        public CDBSettings()
        {
            
        }

    }
}
