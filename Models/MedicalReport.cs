using System.ComponentModel.DataAnnotations;

namespace MADAI_BACKEND.Models
{
    public class MedicalReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public string? AnalysisSummary { get; set; }
        public string? SuggestedConditions { get; set; }
        public string? NextSteps { get; set; }

        public DateTime DateUploaded { get; set; } = DateTime.Now;
    }
}
