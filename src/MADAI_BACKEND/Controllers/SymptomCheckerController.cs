using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Data;
using MADAI_BACKEND.Models;
using MADAI_BACKEND.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace MADAI_BACKEND.Controllers
{
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

            // Save user input as a new SymptomEntry.
            var entry = new SymptomEntry
            {
                PatientName = entryDto.PatientName,
                SymptomsText = entryDto.SymptomsText,
                DateSubmitted = entryDto.DateSubmitted
            };

            _context.SymptomEntries.Add(entry);
            await _context.SaveChangesAsync();

            // Get analysis result from the AI service using the DTO.
            var resultDto = await _symptomService.AnalyzeSymptomsAsync(entryDto);

            // Create the AnalysisResult entity, associating it with the saved SymptomEntry.
            var analysisResult = new AnalysisResult
            {
                Summary = resultDto.Summary,
                SuggestedConditions = string.Join(",", resultDto.SuggestedConditions ?? Array.Empty<string>()),
                NextSteps = string.Join(",", resultDto.NextSteps ?? Array.Empty<string>()),
                SymptomEntryId = entry.Id
            };

            _context.AnalysisResults.Add(analysisResult);
            await _context.SaveChangesAsync();

            return Ok(resultDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetResult(int id)
        {
            var result = await _context.AnalysisResults.FirstOrDefaultAsync(r => r.SymptomEntryId == id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}
