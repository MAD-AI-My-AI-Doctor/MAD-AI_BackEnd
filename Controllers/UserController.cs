using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Data;
using MADAI_BACKEND.Models;
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
        private readonly IMedicalHistoryService _medicalHistoryService;

        public UserController(IMedicalHistoryService medicalHistoryService, AppDbContext context)
        {
            _medicalHistoryService = medicalHistoryService;
            _context = context;
           
        }

        // 🔐 Admin: Get all users
        [Authorize(Roles = "Admin")]
        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // 🔐 Admin: Update any user by ID
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
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

        // 🔐 Admin: Delete any user
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "User deleted successfully." });
        }

        // 👤 Patient: Get own profile
        [Authorize(Roles = "Patient")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            return user == null ? NotFound() : Ok(user);
        }

        // 👤 Patient: Update own profile
        [Authorize(Roles = "Patient")]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] User updated)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.FirstName = updated.FirstName;
            user.LastName = updated.LastName;
            user.Email = updated.Email;

            await _context.SaveChangesAsync();
            return Ok(user);
        }

        // 👤 Patient: Delete own profile
        [Authorize(Roles = "Patient")]
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMyProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Your profile was deleted." });
        }

        [HttpGet("medical-history")]
        public async Task<IActionResult> GetMedicalHistory()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);
            var history = await _medicalHistoryService.GetMedicalHistoryAsync(userId);
            return Ok(history);
        }

        [HttpGet("medical-report/{id}/download")]
        public async Task<IActionResult> DownloadMedicalReport(int id)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var report = await _context.MedicalReports.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
            if (report == null || string.IsNullOrEmpty(report.FilePath)) return NotFound("Medical report not found.");

            var path = Path.Combine(Directory.GetCurrentDirectory(), "uploads", report.FilePath);
            if (!System.IO.File.Exists(path)) return NotFound("File not found on server.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(path);
            return File(fileBytes, "application/pdf", Path.GetFileName(path));
        }
    }
}
