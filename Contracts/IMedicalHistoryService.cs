using MADAI_BACKEND.Models.DTO;

namespace MADAI_BACKEND.Contracts
{
    public interface IMedicalHistoryService
    {
        Task<MedicalHistoryDTO> GetMedicalHistoryAsync(Guid userId);
    }
}
