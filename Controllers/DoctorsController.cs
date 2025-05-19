using MADAI_BACKEND.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MADAI_BACKEND.Controllers
{
    [Authorize(Roles = "Patient")]
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchDoctors([FromQuery] string location, [FromQuery] string specialty)
        {
            if (string.IsNullOrWhiteSpace(location) || string.IsNullOrWhiteSpace(specialty))
                return BadRequest("Location and specialty are required.");

            var results = await _doctorService.SearchDoctorsAsync(location, specialty);
            return Ok(results);
        }
    }
}
