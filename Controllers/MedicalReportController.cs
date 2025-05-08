using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Models;
using MADAI_BACKEND.Models.DTO;
using MADAI_BACKEND.Data;
using Microsoft.EntityFrameworkCore;

namespace MADAI_BACKEND.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalReportController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMedicalReportService _reportService;

        public MedicalReportController(AppDbContext context, IMedicalReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        [HttpPost("upload-report")]
        public async Task<IActionResult> UploadReport([FromForm] MedicalReportDTO reportDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var analysisResult = await _reportService.AnalyzeMedicalReportAsync(reportDto);

            var report = new MedicalReport
            {
                PatientName = reportDto.PatientName,
                FileName = reportDto.File.FileName,
                FilePath = $"/uploads/{reportDto.File.FileName}",
                AnalysisSummary = analysisResult.Summary,
                SuggestedConditions = string.Join(",", analysisResult.SuggestedConditions ?? new string[] { }),
                NextSteps = string.Join(",", analysisResult.NextSteps ?? new string[] { })
            };

            _context.MedicalReports.Add(report);
            await _context.SaveChangesAsync();

            return Ok(analysisResult);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReport(int id)
        {
            var report = await _context.MedicalReports.FindAsync(id);
            if (report == null)
                return NotFound();

            var resultDto = new MedicalReportResultDTO
            {
                Summary = report.AnalysisSummary,
                SuggestedConditions = report.SuggestedConditions?.Split(',') ?? new string[] { },
                NextSteps = report.NextSteps?.Split(',') ?? new string[] { }
            };

            return Ok(resultDto);
        }
    }
}
