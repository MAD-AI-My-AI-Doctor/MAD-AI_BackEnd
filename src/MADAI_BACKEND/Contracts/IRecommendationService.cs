using MADAI_BACKEND.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MADAI_BACKEND.Contracts
{
    /// <summary>
    /// Given a diagnosed disease and a location, 
    /// suggests the right specialist type and returns matching doctors.
    /// </summary>
    public interface IRecommendationService
    {
        Task<IEnumerable<DoctorDto>> RecommendDoctorsByDiseaseAsync(string disease, string location);
    }
}
