// Controllers/HealthController.cs
using System.Threading.Tasks;
using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace MADAI_BACKEND.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IHealthRecommendationService _recommendationService;
        private readonly IHealthInsightsService _insightService;

        public HealthController(
            IHealthRecommendationService recommendationService,
            IHealthInsightsService insightService)
        {
            _recommendationService = recommendationService;
            _insightService = insightService;
        }

        /// <summary>
        /// Story 11: AI Health Recommendations
        /// </summary>
        [HttpPost("recommendations")]
        public async Task<ActionResult<HealthRecommendationResponseDto>> PostRecommendations(
            [FromBody] HealthRecommendationRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _recommendationService.GetHealthRecommendationsAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Story 12: Personalized Health Insights
        /// </summary>
        [HttpPost("insights")]
        public async Task<ActionResult<PersonalizedHealthInsightsResponseDto>> PostInsights(
            [FromBody] PersonalizedHealthInsightsRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _insightService.GetPersonalizedInsightsAsync(request);
            return Ok(result);
        }
    }
}
