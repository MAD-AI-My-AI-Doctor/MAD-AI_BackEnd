using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MADAI_BACKEND.Services
{
    public class AIRecommendationService : IAIRecommendationService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AIRecommendationService(AppDbContext context, HttpClient httpClient, IConfiguration config)
        {
            _context = context;
            _httpClient = httpClient;
            _apiKey = config["OpenRouter:ApiKey"];
        }

        public async Task<string> GenerateHealthAdviceAsync(Guid userId)
        {
            var symptoms = await _context.SymptomEntries
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.DateSubmitted)
                .Take(5)
                .Select(s => s.SymptomsText)
                .ToListAsync();

            var history = string.Join("\n", symptoms);

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You are an AI health advisor." },
                    new { role = "user", content = $"Patient symptoms: {history}\nPlease provide health advice." }
                }
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsJsonAsync("https://openrouter.ai/api/v1/chat/completions", requestBody);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<JsonElement>();
            var advice = content.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return advice ?? "No advice generated.";
        }

        public async Task<string> GeneratePersonalizedHealthInsightsAsync(Guid userId)
        {
            var symptoms = await _context.SymptomEntries
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.DateSubmitted)
                .Select(s => s.SymptomsText)
                .ToListAsync();

            var history = string.Join("\n", symptoms);

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You are a personal health assistant. Based on the user's medical history and symptoms, give personalized health tips to help them stay healthy." },
                    new { role = "user", content = $"User recent symptoms: {history}\nGive personalized health insights and tips for better daily health and lifestyle." }
                }
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsJsonAsync("https://openrouter.ai/api/v1/chat/completions", requestBody);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<JsonElement>();
            var tip = content.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return tip ?? "No insights generated.";
        }
    }
}