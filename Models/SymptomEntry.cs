using System;
using System.ComponentModel.DataAnnotations;

namespace MADAI_BACKEND.Models
{
    public class SymptomEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? PatientName { get; set; }  // Marked as nullable; [Required] will still validate at runtime.

        [Required]
        public string? SymptomsText { get; set; }  // Marked as nullable

        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        public AnalysisResult? AnalysisResult { get; set; }
    }
}
