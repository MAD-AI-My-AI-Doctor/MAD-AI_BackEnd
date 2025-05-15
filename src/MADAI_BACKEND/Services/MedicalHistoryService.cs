using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Data;
using MADAI_BACKEND.Models.DTO;
using MADAI_BACKEND.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MADAI_BACKEND.Services
{
    public class MedicalHistoryService : IMedicalHistoryService
    {
        private readonly AppDbContext _context;
        private readonly string _storagePath;

        public MedicalHistoryService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _storagePath = configuration.GetValue<string>("MedicalHistoryStoragePath") ?? "Uploads/MedicalHistory";
            Directory.CreateDirectory(_storagePath);
        }

        public async Task<MedicalHistoryEntryDTO> UploadEntryAsync(MedicalHistoryEntryCreateDTO dto)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.File.FileName)}";
            var filePath = Path.Combine(_storagePath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var entity = new MedicalHistoryEntry
            {
                PatientId = dto.PatientId,
                Title = dto.Title,
                Description = dto.Description,
                FilePath = fileName,
                UploadedAt = DateTime.UtcNow
            };

            _context.MedicalHistoryEntries.Add(entity);
            await _context.SaveChangesAsync();

            return new MedicalHistoryEntryDTO
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                Title = entity.Title,
                Description = entity.Description,
                FileUrl = $"/api/medicalhistory/{entity.Id}/file",
                UploadedAt = entity.UploadedAt
            };
        }

        public async Task<IEnumerable<MedicalHistoryEntryDTO>> GetEntriesAsync(Guid patientId)
        {
            var entries = await _context.MedicalHistoryEntries
                .Where(e => e.PatientId == patientId)
                .OrderByDescending(e => e.UploadedAt)
                .ToListAsync();

            return entries.Select(e => new MedicalHistoryEntryDTO
            {
                Id = e.Id,
                PatientId = e.PatientId,
                Title = e.Title,
                Description = e.Description,
                FileUrl = $"/api/medicalhistory/{e.Id}/file",
                UploadedAt = e.UploadedAt
            });
        }


        public async Task<List<MedicalHistoryEntryDTO>> GetByPatientIdAsync(
           Guid patientId)
        {
            var entries = await _context.MedicalHistoryEntries
                .Where(e => e.PatientId == patientId)
                .OrderByDescending(e => e.UploadedAt)
                .ToListAsync();

            return entries.Select(e => new MedicalHistoryEntryDTO
            {
                Id = e.Id,
                PatientId = e.PatientId,
                Title = e.Title,
                Description = e.Description,
                FileUrl = $"/api/medicalhistory/{e.Id}/file",
                UploadedAt = e.UploadedAt
            })
            .ToList();
        }


    }
}