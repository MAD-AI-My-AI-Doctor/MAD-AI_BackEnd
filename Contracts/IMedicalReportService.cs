using MADAI_BACKEND.Models.DTO;

namespace MADAI_BACKEND.Contracts
{
    public interface IMedicalReportService
    {
        Task<MedicalReportResultDTO> AnalyzeMedicalReportAsync(MedicalReportDTO report);
    }
}
