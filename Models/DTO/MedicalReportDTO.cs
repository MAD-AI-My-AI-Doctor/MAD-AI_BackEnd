using System.ComponentModel.DataAnnotations;

namespace MADAI_BACKEND.Models.DTO
{
    public class MedicalReportDTO
    {
        [Required]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        public IFormFile File { get; set; } = default!;
    }
}
