using MADAI_BACKEND.Models;
using Microsoft.EntityFrameworkCore;

namespace MADAI_BACKEND.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<SymptomEntry> SymptomEntries { get; set; }
        public DbSet<AnalysisResult> AnalysisResults { get; set; }
        public DbSet<MedicalReport> MedicalReports { get; set; }
        public DbSet<User> Users { get; set; }

        // ✅ Add model relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SymptomEntry>()
                .HasOne(se => se.User)
                .WithMany(u => u.SymptomEntries)
                .HasForeignKey(se => se.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MedicalReport>()
                .HasOne(mr => mr.User)
                .WithMany(u => u.MedicalReports)
                .HasForeignKey(mr => mr.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
