using System;

namespace MADAI_BACKEND.Models.DTO
{
    public class MedicalHistoryEntryDTO
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FileUrl { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}