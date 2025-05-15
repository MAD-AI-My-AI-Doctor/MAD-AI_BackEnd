using MADAI_BACKEND.Data;
using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite; // Add this using directive

var builder = WebApplication.CreateBuilder(args);

// 1. Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// SQL Server
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Or if you're using SQLite for local testing:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddHttpClient();  // Ensure HttpClient factory is available for injection
builder.Services.AddScoped<ISymptomService, SymptomService>();

// Register HttpClient + DoctorService
builder.Services.AddHttpClient<IDoctorService, DoctorService>();
builder.Services.AddScoped<IMedicalHistoryService, MedicalHistoryService>();

// After registering DoctorService:
builder.Services.AddHttpClient<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IHealthRecommendationService, HealthRecommendationService>();
builder.Services.AddScoped<IHealthInsightsService, HealthInsightService>();

// Make sure your appsettings.json has:
// "GoogleMaps": { "ApiKey": "YOUR_GOOGLE_MAPS_API_KEY" }

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2. Enable CORS middleware
app.UseCors("AllowAll");

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
