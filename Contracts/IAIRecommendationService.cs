namespace MADAI_BACKEND.Contracts
{
    public interface IAIRecommendationService
    {
        Task<string> GenerateHealthAdviceAsync(Guid userId);
        Task<string> GeneratePersonalizedHealthInsightsAsync(Guid userId);
    }
}
