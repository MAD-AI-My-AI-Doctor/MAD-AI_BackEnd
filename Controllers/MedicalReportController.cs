using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Models;
using MADAI_BACKEND.Models.DTO;
using MADAI_BACKEND.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MADAI_BACKEND.Controllers
{
    [Authorize] // 🔐 Only logged-in users can access this controller
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

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var analysisResult = await _reportService.AnalyzeMedicalReportAsync(reportDto);

            // ✅ Save file to memory (for DB)
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await reportDto.File.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            // ✅ Ensure 'uploads' folder exists
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            // ✅ Save file to disk
            var filePath = Path.Combine(uploadsDir, reportDto.File.FileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.WriteAsync(fileBytes, 0, fileBytes.Length);
            }

            var report = new MedicalReport
            {
                PatientName = reportDto.PatientName,
                FileName = reportDto.File.FileName,
                FilePath = $"/uploads/{reportDto.File.FileName}",
                FileData = fileBytes,
                AnalysisSummary = analysisResult.Summary,
                SuggestedConditions = string.Join(",", analysisResult.SuggestedConditions ?? new string[] { }),
                NextSteps = string.Join(",", analysisResult.NextSteps ?? new string[] { }),
                UserId = userId
            };

            _context.MedicalReports.Add(report);
            await _context.SaveChangesAsync();

            return Ok(analysisResult);
        }


        [HttpGet("download-report/{id}")]
        public async Task<IActionResult> DownloadReport(int id)
        {
            var report = await _context.MedicalReports.FindAsync(id);

            if (report == null || report.FileData == null)
                return NotFound(new { message = "Report not found or file is missing." });

            return File(report.FileData, "application/pdf", report.FileName);
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

        [HttpGet("my-reports")]
        public async Task<IActionResult> GetMyReports()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var reports = await _context.MedicalReports
                .Where(r => r.UserId == userId)
                .ToListAsync();

            return Ok(reports);
        }

    }
}
