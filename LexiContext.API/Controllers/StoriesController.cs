using LexiContext.Application.DTOs.Stories;
using LexiContext.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LexiContext.API.Controllers
{
    [Controller]
    [Authorize]
    [Route("api/[controller]")]
    public class StoriesController : Controller
    {
        private readonly IStoryService _storyService;
        public StoriesController(IStoryService storyService)
        {
            _storyService = storyService;
        }
        private Guid GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                throw new UnauthorizedAccessException("User ID is not found in token.");
            return Guid.Parse(userIdString);
        }

        [HttpPost("generate")]
        public async Task<ActionResult<StoryDto>> GenerateStory([FromBody] GenerateStoryDto requestDto)
        {
            var userId = GetUserId();
            var result = await _storyService.GenerateStoryAsync(requestDto, userId);

            return CreatedAtAction(nameof(GetStoryById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StoryDto>> GetStoryById(Guid id)
        {
            var userId = GetUserId();

            var result = await _storyService.GetStoryByIdAsync(id, userId);
            return Ok(result);
        }

        [HttpGet("my-stories")]
        public async Task<ActionResult<List<StoryDto>>> GetMyStories()
        {
            var userId = GetUserId();
            var result = await _storyService.GetUserStoriesAsync(userId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStory(Guid id)
        {
            var userId = GetUserId();
            await _storyService.DeleteStoryAsync(id, userId);
            
            return NoContent();
        }
    }
}
