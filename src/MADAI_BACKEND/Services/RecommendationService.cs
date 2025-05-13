using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Models.DTO;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MADAI_BACKEND.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly HttpClient _http;
        private readonly IDoctorService _doctorService;
        private readonly string _openRouterKey;

        public RecommendationService(
            HttpClient http,
            IDoctorService doctorService,
            IConfiguration config)
        {
            _http = http;
            _doctorService = doctorService;
            _openRouterKey = config["OpenRouter:ApiKey"]
                              ?? throw new ArgumentNullException("OpenRouter:ApiKey");
        }

        public async Task<IEnumerable<DoctorDto>> RecommendDoctorsByDiseaseAsync(string disease, string location)
        {
            // 1) Ask Deepseek/OpenRouter: "Which specialty treats {disease}?"
            var payload = new
            {
                model = "deepseek/deepseek-r1-zero:free",
                messages = new[] {
                    new { role = "user", content = $"Which medical specialty is best suited for treating \"{disease}\"?" }
                }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _openRouterKey);

            using var res = await _http.SendAsync(req);
            if (!res.IsSuccessStatusCode)
                throw new ApplicationException($"OpenRouter API failed with {res.StatusCode}");

            using var doc = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
            var choice = doc.RootElement
                              .GetProperty("choices")[0]
                              .GetProperty("message")
                              .GetProperty("content")
                              .GetString()!
                              .Trim().TrimEnd('.');

            // 2) Use the returned specialty to search for doctors
            return await _doctorService.SearchDoctorsAsync(location, choice);
        }
    }
}
