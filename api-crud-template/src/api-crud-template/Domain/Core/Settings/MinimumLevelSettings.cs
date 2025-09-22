namespace Domain.Core.Settings
{
    public record MinimumLevelSettings
    {
        public string Default { get; set; } = "Information";
        public Dictionary<string, string> Override { get; set; } = new()
        {
            ["Microsoft"] = "Warning",
            ["System"] = "Warning"
        };
    }
}
