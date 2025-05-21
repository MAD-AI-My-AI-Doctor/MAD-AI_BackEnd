using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Data;
using MADAI_BACKEND.Models;
using MADAI_BACKEND.Models.DTO;
using MADAI_BACKEND.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MADAI_BACKEND.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAIRecommendationService _aiService;
        private readonly IMedicalHistoryService _medicalHistoryService;

        public UserController(IMedicalHistoryService medicalHistoryService, AppDbContext context, IAIRecommendationService aiService)
        {
            _medicalHistoryService = medicalHistoryService;
            _context = context;
            _aiService = aiService;
        }

        private Guid? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var id) ? id : null;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.Role = updatedUser.Role;

            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "User deleted successfully." });
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetUserId();
            if (userId is not Guid actualUserId) return Unauthorized();

            var user = await _context.Users.FindAsync(actualUserId);
            return user == null ? NotFound() : Ok(user);
        }

        [Authorize(Roles = "Patient")]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] User updated)
        {
            var userId = GetUserId();
            if (userId is not Guid actualUserId) return Unauthorized();

            var user = await _context.Users.FindAsync(actualUserId);
            if (user == null) return NotFound();

            user.FirstName = updated.FirstName;
            user.LastName = updated.LastName;
            user.Email = updated.Email;

            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [Authorize(Roles = "Patient")]
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMyProfile()
        {
            var userId = GetUserId();
            if (userId is not Guid actualUserId) return Unauthorized();

            var user = await _context.Users.FindAsync(actualUserId);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Your profile was deleted." });
        }

        [HttpGet("medical-history")]
        public async Task<IActionResult> GetMedicalHistory()
        {
            var userId = GetUserId();
            if (userId is not Guid actualUserId) return Unauthorized();

            var history = await _medicalHistoryService.GetMedicalHistoryAsync(actualUserId);
            return Ok(history);
        }

        [HttpGet("medical-report/{id}/download")]
        public async Task<IActionResult> DownloadMedicalReport(Guid id)
        {
            var userId = GetUserId();
            if (userId is not Guid actualUserId) return Unauthorized();

            var report = await _context.MedicalReports
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == actualUserId);

            if (report == null || string.IsNullOrEmpty(report.FilePath))
                return NotFound("Medical report not found.");

            var path = Path.Combine(Directory.GetCurrentDirectory(), "uploads", report.FilePath);
            if (!System.IO.File.Exists(path))
                return NotFound("File not found on server.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(path);
            return File(fileBytes, "application/pdf", Path.GetFileName(path));
        }

        [HttpGet("ai-health-recommendation")]
        public async Task<IActionResult> GetAIHealthRecommendation()
        {
            var userId = GetUserId();
            if (userId is not Guid actualUserId) return Unauthorized();

            var advice = await _aiService.GenerateHealthAdviceAsync(actualUserId);
            return Ok(new AIRecommendationDTO { Advice = advice });
        }

        [HttpGet("ai-personalized-insights")]
        public async Task<IActionResult> GetAIPersonalizedInsights()
        {
            var userId = GetUserId();
            if (userId is not Guid actualUserId) return Unauthorized();

            var insights = await _aiService.GeneratePersonalizedHealthInsightsAsync(actualUserId);
            return Ok(new HealthInsightDTO { Tip = insights });
        }
    }
}
