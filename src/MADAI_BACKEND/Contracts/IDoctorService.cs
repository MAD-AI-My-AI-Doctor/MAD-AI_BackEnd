
using MADAI_BACKEND.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MADAI_BACKEND.Contracts
{
    public interface IDoctorService
    {
        /// <summary>
        /// Search for doctors by city/postal code and specialty.
        /// </summary>
        Task<IEnumerable<DoctorDto>> SearchDoctorsAsync(string location, string specialty);
    }
}
