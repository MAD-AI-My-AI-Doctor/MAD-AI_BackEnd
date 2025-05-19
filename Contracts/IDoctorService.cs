using MADAI_BACKEND.Models.DTO;

namespace MADAI_BACKEND.Contracts
{
    public interface IDoctorService
    {
        Task<List<DoctorDto>> SearchDoctorsAsync(string location, string specialty);
    }
}
