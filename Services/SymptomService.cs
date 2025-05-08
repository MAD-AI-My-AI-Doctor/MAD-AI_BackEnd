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
    public class SymptomService : ISymptomService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public SymptomService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<AnalysisResultDTO> AnalyzeSymptomsAsync(MADAI_BACKEND.Models.DTO.SymptomEntryCreateDTO entry)
        {
            // Construct the prompt messages in OpenAI-compatible format
            var systemMessage = new
            {
                role = "system",
                content = "You are a medical assistant AI. Analyze the symptoms and provide possible conditions and recommendations."
            };
            var userMessage = new
            {
                role = "user",
                content = $"Symptoms: {entry.SymptomsText}"
            };

            var requestBody = new
            {
                model = "deepseek/deepseek-r1-zero:free",
                messages = new[] { systemMessage, userMessage }
            };

            string jsonPayload = JsonSerializer.Serialize(requestBody);

            using var client = _httpClientFactory.CreateClient();
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["OpenRouter:ApiKey"]);
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpRequest.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(httpRequest);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to call symptom analysis service.", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                string errorDetails = await response.Content.ReadAsStringAsync();
                throw new ApplicationException($"OpenRouter API call failed with status code {response.StatusCode}.");
            }

            string responseJson = await response.Content.ReadAsStringAsync();
            using JsonDocument jsonDoc = JsonDocument.Parse(responseJson);
            JsonElement root = jsonDoc.RootElement;

            // Use ?? string.Empty to ensure a non-null string.
            string summaryText = root.GetProperty("choices")[0]
                                      .GetProperty("message")
                                      .GetProperty("content")
                                      .GetString() ?? string.Empty;

            var result = new AnalysisResultDTO
            {
                Summary = summaryText,
                SuggestedConditions = Array.Empty<string>(),
                NextSteps = Array.Empty<string>()
            };

            return result;
        }
    }
}
