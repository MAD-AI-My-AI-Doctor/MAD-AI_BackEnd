// File: Contracts/IMedicalHistoryService.cs
using MADAI_BACKEND.Models.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MADAI_BACKEND.Contracts
{
    public interface IMedicalHistoryService
    {
        // Return the DTO so callers (controller) can see the uploaded record
        Task<MedicalHistoryEntryDTO> UploadEntryAsync(MedicalHistoryEntryCreateDTO dto);

        // Return the strongly typed list
        Task<IEnumerable<MedicalHistoryEntryDTO>> GetEntriesAsync(Guid patientId);

        Task<List<MedicalHistoryEntryDTO>> GetByPatientIdAsync(Guid patientId);
    }
}
