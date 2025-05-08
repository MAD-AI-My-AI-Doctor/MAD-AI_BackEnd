namespace MADAI_BACKEND.Models.DTO
{
    public class AnalysisResultDTO
    {
        public string? Summary { get; set; }
        public string[]? SuggestedConditions { get; set; }
        public string[]? NextSteps { get; set; }
    }
}
