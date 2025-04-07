using MADAI_BACKEND.Models.DTO;
using System.Threading.Tasks;

namespace MADAI_BACKEND.Contracts
{
    public interface ISymptomService
    {
        Task<AnalysisResultDTO> AnalyzeSymptomsAsync(SymptomEntryCreateDTO entry);
    }
}
