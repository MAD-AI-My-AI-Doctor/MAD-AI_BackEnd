using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Models.DTO;
using MADAI_BACKEND.Models;
using Microsoft.EntityFrameworkCore;
using MADAI_BACKEND.Data;

namespace MADAI_BACKEND.Services
{
    public class MedicalHistoryService : IMedicalHistoryService
    {
        private readonly AppDbContext _context;

        public MedicalHistoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MedicalHistoryDTO> GetMedicalHistoryAsync(Guid userId)
        {
            var symptoms = await _context.SymptomEntries
                .Where(s => s.UserId == userId)
                .ToListAsync();

            var reports = await _context.MedicalReports
                .Where(r => r.UserId == userId)
                .ToListAsync();

            var responses = await _context.AnalysisResults
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return new MedicalHistoryDTO
            {
                Symptoms = symptoms,
                Reports = reports,
                AIResponses = responses
            };
        }
    }
}
