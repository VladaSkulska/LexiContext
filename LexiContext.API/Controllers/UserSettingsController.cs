using LexiContext.Application.DTOs.UserSettings;
using LexiContext.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LexiContext.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserSettingsController : ControllerBase
    {
        private readonly IUserSettingsService _settingsService;

        public UserSettingsController(IUserSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("The user is not authorized.");

            return Guid.Parse(userIdClaim);
        }

        [HttpGet]
        public async Task<ActionResult<UserSettingsDto>> Get()
        {
            var settings = await _settingsService.GetSettingsAsync(GetUserId());
            return Ok(settings);
        }

        [HttpPut]
        public async Task<ActionResult<UserSettingsDto>> Update([FromBody] UpdateUserSettingsDto dto)
        {
            var settings = await _settingsService.UpdateSettingsAsync(GetUserId(), dto);
            return Ok(settings);
        }
    }
}
