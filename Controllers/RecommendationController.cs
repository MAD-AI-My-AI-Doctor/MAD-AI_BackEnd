using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MADAI_BACKEND.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recSvc;

        public RecommendationController(IRecommendationService recSvc)
            => _recSvc = recSvc;

        /// <summary>
        /// GET /api/recommendation?disease={disease}&location={cityOrPostal}
        /// </summary>
        [HttpGet]
        public async Task<IEnumerable<DoctorDto>> Get(
            [FromQuery] string disease,
            [FromQuery] string location)
        {
            return await _recSvc.RecommendDoctorsByDiseaseAsync(disease, location);
        }
    }
}
