namespace feacture_flag_application.Models
{
    public sealed class FeatureFlag
    {
        public string Key { get; init; } = string.Empty;
        public bool Enabled { get; set; }
        public string? Description { get; set; }
    }
}
