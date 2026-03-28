using feacture_flag_application.Models;
using feacture_flag_application.Services;
using Xunit;

namespace feacture_flag_application.Tests;

public class FeatureFlagStoreTests
{
    [Fact]
    public async Task EvaluateWithUserOverride_PrioritizesUserOverride()
    {
        var store = new InMemoryFeatureFlagStore();
        await store.CreateFlagAsync(new FeatureFlag { Key = "new-ui", Enabled = false, Description = "New UI" });
        await store.SetUserOverrideAsync("new-ui", "user123", true);

        var evaluation = await store.EvaluateAsync("new-ui", userId: "user123", groupId: "beta");

        Assert.True(evaluation.HasValue);
        Assert.True(evaluation.Value);
    }

    [Fact]
    public async Task EvaluateWithGroupOverride_AppliedWhenNoUserOverride()
    {
        var store = new InMemoryFeatureFlagStore();
        await store.CreateFlagAsync(new FeatureFlag { Key = "new-ui", Enabled = false });
        await store.SetGroupOverrideAsync("new-ui", "beta", true);

        var evaluation = await store.EvaluateAsync("new-ui", userId: null, groupId: "beta");

        Assert.True(evaluation.HasValue);
        Assert.True(evaluation.Value);
    }

    [Fact]
    public async Task EvaluateFallsBackToGlobalDefault_WhenNoOverrides()
    {
        var store = new InMemoryFeatureFlagStore();
        await store.CreateFlagAsync(new FeatureFlag { Key = "new-ui", Enabled = false });

        var evaluation = await store.EvaluateAsync("new-ui");

        Assert.True(evaluation.HasValue);
        Assert.False(evaluation.Value);
    }

    [Fact]
    public async Task EvaluateReturnsNullForMissingFeatureFlag()
    {
        var store = new InMemoryFeatureFlagStore();

        var evaluation = await store.EvaluateAsync("does-not-exist");

        Assert.Null(evaluation);
    }

    [Fact]
    public async Task DeleteFlag_RemovesOverridesAndFeature()
    {
        var store = new InMemoryFeatureFlagStore();
        await store.CreateFlagAsync(new FeatureFlag { Key = "new-ui", Enabled = false });
        await store.SetUserOverrideAsync("new-ui", "user123", true);

        var removed = await store.DeleteFlagAsync("new-ui");
        var evaluation = await store.EvaluateAsync("new-ui", userId: "user123");

        Assert.True(removed);
        Assert.Null(evaluation);
    }
}
