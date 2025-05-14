
using Microsoft.EntityFrameworkCore;
using MADAI_BACKEND.Models;
namespace MADAI_BACKEND.Data
{



public class AppDbContext : DbContext
{


 public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /*  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
       optionsBuilder.UseSqlite("Data Source = MADAI.db");
   }  */

    public DbSet<SymptomEntry> SymptomEntries { get; set; }
    public DbSet<AnalysisResult> AnalysisResults { get; set; }
    public DbSet<MedicalHistoryEntry> MedicalHistoryEntries { get; set; }

    }
}