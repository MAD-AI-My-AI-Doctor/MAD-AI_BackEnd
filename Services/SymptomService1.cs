using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MADAI_BACKEND.Services
{
    public class SymptomService1 
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public SymptomService1(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            // Debugging: log the API key (remove or secure this before production)
            var apiKey = _configuration["OpenAI:ApiKey"];
            Console.WriteLine($"Loaded OpenAI API Key: {apiKey}");
        }

        public async Task<AnalysisResult> AnalyzeSymptomsAsync(SymptomEntry entry)
        {
            // Create a prompt based on the input symptoms.
            var prompt = $"A patient reports the following symptoms: {entry.SymptomsText}. What are the possible medical conditions and next steps?";

            // Build the request body to match the OpenAI API requirements.
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful medical assistant." },
                    new { role = "user", content = prompt }
                }
            };

            // Serialize the request body and create StringContent.
            var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            // Set the Authorization header with your OpenAI API key from appsettings.json.
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _configuration["OpenAI:ApiKey"]);

            // Call the OpenAI API endpoint.
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);
            var responseString = await response.Content.ReadAsStringAsync();

            // Parse the JSON response.
            using var jsonDoc = JsonDocument.Parse(responseString);
            var root = jsonDoc.RootElement;

            // Check if the response contains an error property.
            if (root.TryGetProperty("error", out JsonElement errorElement))
            {
                string errorMessage = errorElement.TryGetProperty("message", out JsonElement msgElement)
                    ? msgElement.GetString()
                    : "Unknown error from OpenAI API.";
                throw new Exception($"OpenAI API error: {errorMessage}");
            }

            // Ensure that the "choices" property exists and contains at least one element.
            if (!root.TryGetProperty("choices", out JsonElement choices) || choices.GetArrayLength() == 0)
            {
                throw new Exception("No choices were returned in the API response: " + responseString);
            }

            // Extract the nested "message" and "content" properties.
            if (!choices[0].TryGetProperty("message", out JsonElement messageElement) ||
                !messageElement.TryGetProperty("content", out JsonElement contentElement))
            {
                throw new Exception("The expected message content was not found in the API response: " + responseString);
            }

            var resultText = contentElement.GetString();

            // Return an AnalysisResult with the data extracted.
            return new AnalysisResult
            {
                Summary = resultText,
                SuggestedConditions = "Check details in summary.",
                NextSteps = "Consult a doctor if symptoms persist."
            };
        }
    }
}
