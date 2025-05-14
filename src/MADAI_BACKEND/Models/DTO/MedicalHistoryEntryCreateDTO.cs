using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace MADAI_BACKEND.Models.DTO
{
    public class MedicalHistoryEntryCreateDTO
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public IFormFile File { get; set; }
    }

}