namespace feacture_flag_application.Models
{
    public sealed record FeatureFlagOverrideKey
    {
        public string FlagKey { get; init; } = string.Empty;
        public string SubjectId { get; init; } = string.Empty; // user or group
    }
}
