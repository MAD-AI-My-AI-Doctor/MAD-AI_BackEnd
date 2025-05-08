
using MADAI_BACKEND.Models;
using Microsoft.EntityFrameworkCore;
namespace MADAI_BACKEND.Data
{



public class AppDbContext : DbContext
{


 public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /*  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
       optionsBuilder.UseSqlite("Data Source = MADAI.db");
   }  */

    public DbSet<Models.SymptomEntry> SymptomEntries { get; set; }
    public DbSet<Models.AnalysisResult> AnalysisResults { get; set; }
        public DbSet<MedicalReport> MedicalReports { get; set; }


    }
}