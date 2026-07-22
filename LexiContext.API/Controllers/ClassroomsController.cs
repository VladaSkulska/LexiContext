using LexiContext.Application.DTOs.Classrooms;
using LexiContext.Application.DTOs.Decks;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace LexiContext.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class ClassroomsController : ControllerBase
    {
        private readonly IClassroomService _classroomService;

        public ClassroomsController(IClassroomService classroomService)
        {
            _classroomService = classroomService;
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")] 
        public async Task<ActionResult<ClassroomDto>> CreateClassroom([FromBody] CreateClassroomDto dto)
        {
            var teacherId = GetUserId();
            var classroom = await _classroomService.CreateClassroomAsync(dto, teacherId);
            return Ok(classroom);
        }

        [HttpGet("teacher")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<List<ClassroomDto>>> GetTeacherClassrooms()
        {
            var teacherId = GetUserId();
            var classrooms = await _classroomService.GetTeacherClassroomsAsync(teacherId);
            return Ok(classrooms);
        }

        [HttpPost("{classroomId}/decks/{deckId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> AddDeckToClassroom(Guid classroomId, Guid deckId)
        {
            try
            {
                var teacherId = GetUserId(); 
                await _classroomService.AddDeckToClassroomAsync(classroomId, deckId, teacherId);

                return Ok(new { Message = "Колоду успішно додано до класу!" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal Server Error." });
            }
        }

        [HttpPost("join")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ClassroomDto>> JoinClassroom([FromBody] JoinClassroomDto dto)
        {
            try
            {
                var studentId = GetUserId();
                var classroom = await _classroomService.JoinClassroomAsync(dto.JoinCode, studentId);
                return Ok(classroom);
            }
            catch (NotFoundException)
            {
                return BadRequest(new { message = "classroom_not_found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal Server Error." });
            }
        }

        [HttpGet("student")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<List<ClassroomDto>>> GetStudentClassrooms()
        {
            var studentId = GetUserId();
            var classrooms = await _classroomService.GetStudentClassroomsAsync(studentId);
            return Ok(classrooms);
        }

        private Guid GetUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim))
                throw new UnauthorizedAccessException("User is not found in token.");

            return Guid.Parse(idClaim);
        }

        [HttpGet("{classroomId}/decks")]
        [Authorize]
        public async Task<ActionResult<List<DeckDto>>> GetClassroomDecks(Guid classroomId)
        {
            try
            {
                var userId = GetUserId();
                var decks = await _classroomService.GetClassroomDecksAsync(classroomId, userId);

                return Ok(decks);
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

        [HttpPost("leave")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> LeaveClassroom([FromBody] LeaveClassroomDto dto)
        {
            try
            {
                var studentId = GetUserId();
                await _classroomService.LeaveClassroomAsync(dto.ClassroomId, studentId);
                return Ok(new { message = "You left this class." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal Server Error." });
            }
        }

        [HttpDelete("{classroomId}/decks/{deckId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> RemoveDeckFromClassroom(Guid classroomId, Guid deckId)
        {
            try
            {
                var teacherId = GetUserId();
                await _classroomService.RemoveDeckFromClassroomAsync(classroomId, deckId, teacherId);
                return Ok(new { message = "Deck is removed." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal Server Error." });
            }
        }

        [HttpDelete("{classroomId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteClassroom(Guid classroomId)
        {
            try
            {
                var teacherId = GetUserId();
                await _classroomService.DeleteClassroomAsync(classroomId, teacherId);
                return Ok(new { message = "Class was deleted." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal Server Error." });
            }
        }

        [HttpPost("{classroomId}/homework")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateHomework(Guid classroomId, [FromBody] CreateHomeworkRequest request)
        {
            try
            {
                var teacherId = GetUserId();
                if (string.IsNullOrWhiteSpace(request?.Text))
                    return BadRequest(new { message = "The task text cannot be empty." });

                await _classroomService.CreateHomeworkAsync(classroomId, request.Text, teacherId);
                return Ok(new { message = "Homework successfully created for all students in the class!" });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpGet("{classroomId}/homework/teacher")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetHomeworkForTeacher(Guid classroomId)
        {
            try
            {
                var teacherId = GetUserId();
                var homework = await _classroomService.GetHomeworkForTeacherAsync(classroomId, teacherId);
                return Ok(homework);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    stack = ex.StackTrace
                });
            }
        }

        [HttpGet("{classroomId}/homework/student")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetHomeworkForStudent(Guid classroomId)
        {
            try
            {
                var studentId = GetUserId();
                var homework = await _classroomService.GetHomeworkForStudentAsync(classroomId, studentId);
                return Ok(homework);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpDelete("homework/{groupTaskId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteHomework(Guid groupTaskId)
        {
            try
            {
                var teacherId = GetUserId();
                await _classroomService.DeleteHomeworkAsync(groupTaskId, teacherId);
                return Ok(new { message = "Homework deleted." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpPatch("homework/{groupTaskId}/toggle")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ToggleHomework(Guid groupTaskId)
        {
            try
            {
                var studentId = GetUserId();
                await _classroomService.ToggleHomeworkAsync(groupTaskId, studentId);
                return Ok(new { message = "Status updated." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        public record CreateHomeworkRequest(string Text);
    }
}
