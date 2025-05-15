using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Models.DTO;
using Microsoft.Extensions.Configuration;
using UglyToad.PdfPig;

namespace MADAI_BACKEND.Services
{
    public class MedicalReportService : IMedicalReportService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public MedicalReportService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<MedicalReportResultDTO> AnalyzeMedicalReportAsync(MedicalReportDTO report)
        {
            var client = _httpClientFactory.CreateClient();
            var apiKey = _configuration.GetValue<string>("OpenRouter:ApiKey");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ApplicationException("OpenRouter API key is not configured.");
            }

            // Extract text from PDF using PdfPig
            string extractedText = ExtractTextFromPdf(report.File);

            // Construct the request body
            var systemMessage = new
            {
                role = "system",
                content = "You are a medical diagnostic assistant.\r\n\r\nCarefully read the attached medical report. Extract all key test values (e.g., blood levels, vitals, imaging results). For each result:\r\n\r\n- Compare it with the normal reference range (if provided or implied).\r\n- Clearly highlight **any values that are too high or too low**.\r\n- Explain the clinical significance briefly and in plain language.\r\n- Provide possible conditions or diagnoses related to these abnormalities.\r\n- Format your findings as clear bullet points for readability.\r\n\r\nBe accurate, concise, and helpful. Assume the report belongs to a general patient unless specific demographics are mentioned. and response in the bullet form."
            };
            var userMessage = new
            {
                role = "user",
                content = $"Patient Name: {report.PatientName}, Report Text: {extractedText}"
            };

            var requestBody = new
            {
                model = "deepseek/deepseek-r1-zero:free",
                messages = new[] { systemMessage, userMessage }
            };

            string jsonPayload = JsonSerializer.Serialize(requestBody);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.SendAsync(request);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new ApplicationException($"API call failed: {response.StatusCode}\nResponse: {responseJson}");
                }

                using var jsonDoc = JsonDocument.Parse(responseJson);
                var summaryText = jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

                return new MedicalReportResultDTO
                {
                    Summary = summaryText,
                    SuggestedConditions = new string[] { },
                    NextSteps = new string[] { }
                };
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to analyze the medical report.", ex);
            }
        }

        private string ExtractTextFromPdf(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var pdfDocument = PdfDocument.Open(stream);
                var textBuilder = new StringBuilder();

                foreach (var page in pdfDocument.GetPages())
                {
                    textBuilder.AppendLine(page.Text);
                }

                return textBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to extract text from PDF.", ex);
            }
        }
    }
}
