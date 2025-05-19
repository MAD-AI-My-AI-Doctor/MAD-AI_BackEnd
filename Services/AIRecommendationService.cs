using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MADAI_BACKEND.Services
{
    public class AIRecommendationService : IAIRecommendationService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        public AIRecommendationService(AppDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<string> GenerateHealthAdviceAsync(int userId)
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
                model = "deepseek-chat",
                messages = new[]
                {
                new { role = "system", content = "You are an AI health advisor." },
                new { role = "user", content = $"Patient symptoms: {history}\nPlease provide health advice." }
            }
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.deepseek.com/v1/chat/completions", requestBody);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<JsonElement>();
            var advice = content.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return advice ?? "No advice generated.";
        }
    }
}
