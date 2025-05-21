using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Data;
using MADAI_BACKEND.Models;
using MADAI_BACKEND.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MADAI_BACKEND.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SymptomCheckerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISymptomService _symptomService;

        public SymptomCheckerController(AppDbContext context, ISymptomService symptomService)
        {
            _context = context;
            _symptomService = symptomService;
        }

        [HttpPost]
        public async Task<IActionResult> PostSymptom([FromBody] SymptomEntryCreateDTO entryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized("Invalid or missing user ID.");

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return BadRequest("User does not exist.");

            var entry = new SymptomEntry
            {
                PatientName = entryDto.PatientName,
                SymptomsText = entryDto.SymptomsText,
                DateSubmitted = entryDto.DateSubmitted,
                UserId = userId
            };

            _context.SymptomEntries.Add(entry);
            await _context.SaveChangesAsync();

            var resultDto = await _symptomService.AnalyzeSymptomsAsync(entryDto);

            var analysisResult = new AnalysisResult
            {
                Summary = resultDto.Summary,
                SuggestedConditions = string.Join(",", resultDto.SuggestedConditions ?? Array.Empty<string>()),
                NextSteps = string.Join(",", resultDto.NextSteps ?? Array.Empty<string>()),
                SymptomEntryId = entry.Id,
                UserId = userId
            };

            _context.AnalysisResults.Add(analysisResult);
            await _context.SaveChangesAsync();

            return Ok(resultDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetResult(Guid id)
        {
            var result = await _context.AnalysisResults
                .FirstOrDefaultAsync(r => r.SymptomEntryId == id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("my-symptoms")]
        public async Task<IActionResult> GetMySymptoms()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized("Invalid or missing user ID.");

            var entries = await _context.SymptomEntries
                .Where(e => e.UserId == userId)
                .ToListAsync();

            return Ok(entries);
        }
    }
}
