using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MADAI_BACKEND.Models
{
    public class AnalysisResult
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("SymptomEntry")]
        public int SymptomEntryId { get; set; }

        // Mark as nullable so the compiler won’t warn if not set in the constructor.
        public string? Summary { get; set; }
        public string? SuggestedConditions { get; set; }
        public string? NextSteps { get; set; }

        public SymptomEntry? SymptomEntry { get; set; }
    }
}

