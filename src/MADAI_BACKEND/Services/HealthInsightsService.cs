// Services/HealthInsightService.cs
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Models.DTO;
using Microsoft.Extensions.Configuration;

namespace MADAI_BACKEND.Services
{
    public class HealthInsightService : IHealthInsightsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public HealthInsightService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<PersonalizedHealthInsightsResponseDto> GetPersonalizedInsightsAsync(
            PersonalizedHealthInsightsRequestDto request)
        {
            var systemMessage = new
            {
                role = "system",
                content = "You are a healthcare AI assistant. " +
                          "Provide personalized health tips and alerts based on the patient's symptoms and medical history."
            };

            var userBuilder = new StringBuilder();
            userBuilder.AppendLine($"Symptoms: {request.Symptoms}");
            if (request.MedicalHistory.Count > 0)
            {
                userBuilder.AppendLine("Medical History:");
                foreach (var entry in request.MedicalHistory)
                {
                    userBuilder.AppendLine(
                        $"- {entry.UploadedAt:d}: {entry.Description}");
                }
            }

            var payload = new
            {
                model = "deepseek/deepseek-r1-zero:free",
                messages = new[] { systemMessage, new { role = "user", content = userBuilder.ToString() } }
            };

            string jsonPayload = JsonSerializer.Serialize(payload);

            using var client = _httpClientFactory.CreateClient();
            using var httpReq = new HttpRequestMessage(
                HttpMethod.Post,
                "https://openrouter.ai/api/v1/chat/completions");
            httpReq.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _configuration["OpenRouter:ApiKey"]);
            httpReq.Content = new StringContent(
                jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(httpReq);
            response.EnsureSuccessStatusCode();

            string respJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(respJson);
            string content = doc.RootElement
                                .GetProperty("choices")[0]
                                .GetProperty("message")
                                .GetProperty("content")
                                .GetString() ?? string.Empty;

            return new PersonalizedHealthInsightsResponseDto
            {
                Insights = content.Trim()
            };
        }
    }
}
