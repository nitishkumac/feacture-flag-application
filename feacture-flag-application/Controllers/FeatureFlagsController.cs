using feacture_flag_application.Models;
using feacture_flag_application.Services;
using Microsoft.AspNetCore.Mvc;

namespace feacture_flag_application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeatureFlagsController : ControllerBase
    {
        private readonly IFeatureFlagStore _store;

        public FeatureFlagsController(IFeatureFlagStore store) => _store = store;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _store.ListFlagsAsync());

        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            var flag = await _store.GetFlagAsync(key);
            return flag == null ? NotFound(new { error = "Feature flag not found" }) : Ok(flag);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FeatureFlag flag)
        {
            if (flag == null || string.IsNullOrWhiteSpace(flag.Key))
                return BadRequest(new { error = "Feature flag key is required" });

            try
            {
                var created = await _store.CreateFlagAsync(flag);
                return CreatedAtAction(nameof(Get), new { key = created.Key }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpPut("{key}")]
        public async Task<IActionResult> Update(string key, [FromBody] FeatureFlag flag)
        {
            if (flag == null) return BadRequest(new { error = "Request body is required" });

            var updated = await _store.UpdateFlagAsync(key, flag.Enabled, flag.Description);
            return updated == null ? NotFound(new { error = "Feature flag not found" }) : Ok(updated);
        }

        [HttpDelete("{key}")]
        public async Task<IActionResult> Delete(string key)
        {
            var deleted = await _store.DeleteFlagAsync(key);
            return deleted ? NoContent() : NotFound(new { error = "Feature flag not found" });
        }

        [HttpPost("{key}/evaluate")]
        public async Task<IActionResult> Evaluate(string key, [FromQuery] string? userId, [FromQuery] string? groupId)
        {
            var evaluation = await _store.EvaluateAsync(key, userId, groupId);
            if (evaluation == null) return NotFound(new { error = "Feature flag not found" });
            return Ok(new { key, enabled = evaluation, userId, groupId });
        }

        [HttpPost("{key}/override/user")]
        public async Task<IActionResult> SetUserOverride(string key, [FromQuery] string userId, [FromQuery] bool enabled)
        {
            if (string.IsNullOrWhiteSpace(userId)) return BadRequest(new { error = "userId is required" });
            var ok = await _store.SetUserOverrideAsync(key, userId, enabled);
            return ok ? Ok(new { key, userId, enabled }) : NotFound(new { error = "Feature flag not found" });
        }

        [HttpPost("{key}/override/group")]
        public async Task<IActionResult> SetGroupOverride(string key, [FromQuery] string groupId, [FromQuery] bool enabled)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return BadRequest(new { error = "groupId is required" });
            var ok = await _store.SetGroupOverrideAsync(key, groupId, enabled);
            return ok ? Ok(new { key, groupId, enabled }) : NotFound(new { error = "Feature flag not found" });
        }

        [HttpDelete("{key}/override/user")]
        public async Task<IActionResult> RemoveUserOverride(string key, [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return BadRequest(new { error = "userId is required" });
            var removed = await _store.RemoveUserOverrideAsync(key, userId);
            return removed ? NoContent() : NotFound(new { error = "Override not found" });
        }

        [HttpDelete("{key}/override/group")]
        public async Task<IActionResult> RemoveGroupOverride(string key, [FromQuery] string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return BadRequest(new { error = "groupId is required" });
            var removed = await _store.RemoveGroupOverrideAsync(key, groupId);
            return removed ? NoContent() : NotFound(new { error = "Override not found" });
        }
    }
}
