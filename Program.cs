using MADAI_BACKEND.Data;
using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite; // Add this using directive

var builder = WebApplication.CreateBuilder(args);

// SQL Server
// builder.Services.AddDbContext<AppDbContext>(options =>
// options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Or if you're using SQLite for local testing:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient();
builder.Services.AddScoped<IMedicalReportService, MedicalReportService>();


//builder.Services.AddHttpClient<ISymptomService, SymptomService>();

builder.Services.AddControllers();

builder.Services.AddHttpClient();  // Ensure HttpClient factory is available for injection
//builder.Services.AddScoped<SymptomService>();  // Register SymptomService for DI (was SymptomService1 before, now using the updated SymptomService)
builder.Services.AddScoped<ISymptomService, SymptomService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
