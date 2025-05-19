namespace MADAI_BACKEND.Contracts
{
    public interface IAIRecommendationService
    {
        Task<string> GenerateHealthAdviceAsync(int userId);
        Task<string> GeneratePersonalizedHealthInsightsAsync(int userId);
    }
}
