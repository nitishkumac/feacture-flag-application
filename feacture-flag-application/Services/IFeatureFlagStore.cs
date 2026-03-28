using feacture_flag_application.Models;

namespace feacture_flag_application.Services
{
    public interface IFeatureFlagStore
    {
        Task<IEnumerable<FeatureFlag>> ListFlagsAsync();
        Task<FeatureFlag?> GetFlagAsync(string key);
        Task<FeatureFlag> CreateFlagAsync(FeatureFlag flag);
        Task<FeatureFlag?> UpdateFlagAsync(string key, bool enabled, string? description);
        Task<bool> DeleteFlagAsync(string key);

        Task<bool> SetUserOverrideAsync(string key, string userId, bool enabled);
        Task<bool> SetGroupOverrideAsync(string key, string groupId, bool enabled);
        Task<bool> RemoveUserOverrideAsync(string key, string userId);
        Task<bool> RemoveGroupOverrideAsync(string key, string groupId);

        Task<bool?> EvaluateAsync(string key, string? userId = null, string? groupId = null);
    }
}
