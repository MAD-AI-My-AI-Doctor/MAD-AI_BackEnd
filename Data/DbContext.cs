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
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // SymptomEntry -> User (One-to-Many)
            modelBuilder.Entity<SymptomEntry>()
                .HasOne(se => se.User)
                .WithMany(u => u.SymptomEntries)
                .HasForeignKey(se => se.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // MedicalReport -> User (One-to-Many)
            modelBuilder.Entity<MedicalReport>()
                .HasOne(mr => mr.User)
                .WithMany(u => u.MedicalReports)
                .HasForeignKey(mr => mr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // AnalysisResult -> User (One-to-Many)
            modelBuilder.Entity<AnalysisResult>()
                .HasOne(ar => ar.User)
                .WithMany()
                .HasForeignKey(ar => ar.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // AnalysisResult -> SymptomEntry (One-to-One)
            modelBuilder.Entity<AnalysisResult>()
                .HasOne(ar => ar.SymptomEntry)
                .WithOne(se => se.AnalysisResult)
                .HasForeignKey<AnalysisResult>(ar => ar.SymptomEntryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
