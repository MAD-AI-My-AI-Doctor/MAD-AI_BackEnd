using System;
using System.ComponentModel.DataAnnotations;

namespace MADAI_BACKEND.Models.DTO
{
    public class SymptomEntryCreateDTO
    {
        [Required]
        public string? PatientName { get; set; }

        [Required]
        public string? SymptomsText { get; set; }

        public DateTime DateSubmitted { get; set; } = DateTime.Now;
    }
}
