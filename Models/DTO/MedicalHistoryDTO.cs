namespace MADAI_BACKEND.Models.DTO
{
    public class MedicalHistoryDTO
    {
        public List<SymptomEntry> Symptoms { get; set; } = new();
        public List<MedicalReport> Reports { get; set; } = new();
        public List<AnalysisResult> AIResponses { get; set; } = new();
    }
}
