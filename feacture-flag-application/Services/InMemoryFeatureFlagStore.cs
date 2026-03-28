using System.Collections.Concurrent;
using feacture_flag_application.Models;

namespace feacture_flag_application.Services
{
    public sealed class InMemoryFeatureFlagStore : IFeatureFlagStore
    {
        private readonly ConcurrentDictionary<string, FeatureFlag> _flags = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<FeatureFlagOverrideKey, bool> _userOverrides = new();
        private readonly ConcurrentDictionary<FeatureFlagOverrideKey, bool> _groupOverrides = new();

        public Task<IEnumerable<FeatureFlag>> ListFlagsAsync() => Task.FromResult(_flags.Values.AsEnumerable());

        public Task<FeatureFlag?> GetFlagAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return Task.FromResult<FeatureFlag?>(null);
            _flags.TryGetValue(key.Trim(), out var flag);
            return Task.FromResult(flag);
        }

        public Task<FeatureFlag> CreateFlagAsync(FeatureFlag flag)
        {
            if (flag == null) throw new ArgumentNullException(nameof(flag));
            if (string.IsNullOrWhiteSpace(flag.Key)) throw new ArgumentException("Key is required", nameof(flag));
            var key = flag.Key.Trim();

            if (!_flags.TryAdd(key, new FeatureFlag { Key = key, Enabled = flag.Enabled, Description = flag.Description }))
                throw new InvalidOperationException($"Feature flag '{key}' already exists");

            return Task.FromResult(_flags[key]);
        }

        public Task<FeatureFlag?> UpdateFlagAsync(string key, bool enabled, string? description)
        {
            if (string.IsNullOrWhiteSpace(key)) return Task.FromResult<FeatureFlag?>(null);
            var trimmed = key.Trim();
            if (!_flags.TryGetValue(trimmed, out var flag)) return Task.FromResult<FeatureFlag?>(null);

            flag.Enabled = enabled;
            flag.Description = description;
            return Task.FromResult<FeatureFlag?>(flag);
        }

        public Task<bool> DeleteFlagAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return Task.FromResult(false);
            var trimmed = key.Trim();
            var removed = _flags.TryRemove(trimmed, out _);
            if (removed)
            {
                var userKeys = _userOverrides.Keys.Where(o => o.FlagKey.Equals(trimmed, StringComparison.OrdinalIgnoreCase)).ToArray();
                foreach (var k in userKeys) _userOverrides.TryRemove(k, out _);
                var groupKeys = _groupOverrides.Keys.Where(o => o.FlagKey.Equals(trimmed, StringComparison.OrdinalIgnoreCase)).ToArray();
                foreach (var k in groupKeys) _groupOverrides.TryRemove(k, out _);
            }
            return Task.FromResult(removed);
        }

        public Task<bool> SetUserOverrideAsync(string key, string userId, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(userId)) return Task.FromResult(false);
            var flagExists = _flags.ContainsKey(key.Trim());
            if (!flagExists) return Task.FromResult(false);

            var overrideKey = new FeatureFlagOverrideKey { FlagKey = key.Trim(), SubjectId = userId.Trim() };
            _userOverrides[overrideKey] = enabled;
            return Task.FromResult(true);
        }

        public Task<bool> SetGroupOverrideAsync(string key, string groupId, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(groupId)) return Task.FromResult(false);
            var flagExists = _flags.ContainsKey(key.Trim());
            if (!flagExists) return Task.FromResult(false);

            var overrideKey = new FeatureFlagOverrideKey { FlagKey = key.Trim(), SubjectId = groupId.Trim() };
            _groupOverrides[overrideKey] = enabled;
            return Task.FromResult(true);
        }

        public Task<bool> RemoveUserOverrideAsync(string key, string userId)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(userId)) return Task.FromResult(false);
            var result = _userOverrides.TryRemove(new FeatureFlagOverrideKey { FlagKey = key.Trim(), SubjectId = userId.Trim() }, out _);
            return Task.FromResult(result);
        }

        public Task<bool> RemoveGroupOverrideAsync(string key, string groupId)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(groupId)) return Task.FromResult(false);
            var result = _groupOverrides.TryRemove(new FeatureFlagOverrideKey { FlagKey = key.Trim(), SubjectId = groupId.Trim() }, out _);
            return Task.FromResult(result);
        }

        public Task<bool?> EvaluateAsync(string key, string? userId = null, string? groupId = null)
        {
            if (string.IsNullOrWhiteSpace(key)) return Task.FromResult<bool?>(null);
            var trimmed = key.Trim();
            if (!_flags.TryGetValue(trimmed, out var flag)) return Task.FromResult<bool?>(null);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                var userKey = new FeatureFlagOverrideKey { FlagKey = trimmed, SubjectId = userId.Trim() };
                if (_userOverrides.TryGetValue(userKey, out var userOverride)) return Task.FromResult<bool?>(userOverride);
            }

            if (!string.IsNullOrWhiteSpace(groupId))
            {
                var groupKey = new FeatureFlagOverrideKey { FlagKey = trimmed, SubjectId = groupId.Trim() };
                if (_groupOverrides.TryGetValue(groupKey, out var groupOverride)) return Task.FromResult<bool?>(groupOverride);
            }

            return Task.FromResult<bool?>(flag.Enabled);
        }
    }
}
