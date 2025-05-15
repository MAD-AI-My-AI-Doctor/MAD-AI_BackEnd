namespace MADAI_BACKEND.Contracts
{
    using MADAI_BACKEND.Models.DTO;
    public interface IHealthRecommendationService
    {
        Task<HealthRecommendationResponseDto> GetHealthRecommendationsAsync(HealthRecommendationRequestDto request);
    }
}