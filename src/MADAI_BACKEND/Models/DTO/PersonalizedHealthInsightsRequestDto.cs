using System;
using System.ComponentModel.DataAnnotations;
using MADAI_BACKEND.Models.DTO;

namespace MADAI_BACKEND.Models.DTO

{
    public class PersonalizedHealthInsightsRequestDto
    {
        [Required]
        public string Symptoms { get; set; } = string.Empty;

        [Required]
        public List<MedicalHistoryEntryDTO> MedicalHistory { get; set; } = new();
    }
}