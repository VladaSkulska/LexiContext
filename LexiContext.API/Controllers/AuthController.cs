using LexiContext.API.DTOs;
using LexiContext.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiContext.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Token))
            {
                return BadRequest(new { message = "Token is reqired." });
            }

            try
            {
                var jwtToken = await _authService.LoginWithGoogleAsync(request.Token);

                return Ok(new { token = jwtToken });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("role")]
        [Authorize]
        public async Task<IActionResult> UpdateRole([FromBody] string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return BadRequest(new { message = "Role is required." });
            }

            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "User is not authorized." });
                }

                var userId = Guid.Parse(userIdClaim);

                var newToken = await _authService.UpdateUserRoleAsync(userId, role);

                return Ok(new { token = newToken });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}