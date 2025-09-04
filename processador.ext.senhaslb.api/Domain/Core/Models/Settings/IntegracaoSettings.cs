namespace Domain.Core.Models.Settings
{
    public class IntegracaoSettings
    {
        public SAConfig SA { get; set; } = new SAConfig();
    }

    public class SAConfig
    {
        public string Url { get; set; } = string.Empty;
    }
}
