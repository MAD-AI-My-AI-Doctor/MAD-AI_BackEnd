using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MADAI_BACKEND.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        /// <summary>
        /// GET /api/doctors/search?location={cityOrPostal}&specialty={type}
        /// </summary>
        [HttpGet("search")]
        public async Task<IEnumerable<DoctorDto>> Search([FromQuery] string location, [FromQuery] string specialty)
        {
            return await _doctorService.SearchDoctorsAsync(location, specialty);
        }
    }
}
