namespace MADAI_BACKEND.Contracts
{
    using MADAI_BACKEND.Models.DTO;
    public interface IHealthInsightsService
    {
        Task<PersonalizedHealthInsightsResponseDto> GetPersonalizedInsightsAsync(PersonalizedHealthInsightsRequestDto request);
    }
}
