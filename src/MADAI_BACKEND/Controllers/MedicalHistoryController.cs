using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace MADAI_BACKEND.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalHistoryController : ControllerBase
    {
        private readonly IMedicalHistoryService _service;

        public MedicalHistoryController(IMedicalHistoryService service)
        {
            _service = service;
        }

        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> Upload([FromForm] MedicalHistoryEntryCreateDTO dto)
        {
            var result = await _service.UploadEntryAsync(dto);
            return CreatedAtAction(nameof(GetEntries), new { patientId = result.PatientId }, result);
        }

        [HttpGet]
        //[Authorize]
        public async Task<IActionResult> GetEntries([FromQuery] Guid patientId)
        {
            var entries = await _service.GetEntriesAsync(patientId);
            return Ok(entries);
        }

        [HttpGet("{id}/file")]
//        [Authorize]
        public IActionResult DownloadFile(Guid id)
        {
            // Implementation for streaming the file by id, omitted for brevity.
            return NotFound();
        }
    }
}