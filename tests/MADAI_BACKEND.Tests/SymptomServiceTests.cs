// Tests/SymptomServiceTests.cs
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;
using MADAI_BACKEND.Services;
using MADAI_BACKEND.Models.DTO;
using MADAI_BACKEND.Tests.Helpers; // if you keep FakeHttpMessageHandler here
using System.Net.Http.Headers;

namespace MADAI_BACKEND.Tests
{
    /// <summary>
    /// A tiny IHttpClientFactory implementation that always returns the same HttpClient.
    /// </summary>
    class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;
        public FakeHttpClientFactory(HttpClient client) => _client = client;
        public HttpClient CreateClient(string name) => _client;
    }

    public class SymptomServiceTests
    {
        [Fact]
        public async Task AnalyzeSymptomsAsync_ReturnsExpectedSummary()
        {
            // Arrange: fake OpenRouter JSON response
            var fakeJson = @"{
                ""choices"": [
                    { ""message"": { ""content"": ""You may have a headache"" } }
                ]
            }";

            // Fake message handler to return our JSON
            var handler = new FakeHttpMessageHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(fakeJson, Encoding.UTF8, "application/json")
                });
            var httpClient = new HttpClient(handler)
            {
                // if your service names clients, set a default name
                DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } }
            };

            // Wrap our client in the fake factory
            var factory = new FakeHttpClientFactory(httpClient);

            // In-memory configuration with your test API key
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string,string?>("OpenRouter:ApiKey","test-key")
                })
                .Build();

            // Act: construct the service with the factory + config
            var service = new SymptomService(factory, config);
            var entryDto = new SymptomEntryCreateDTO
            {
                PatientName = "Alice",
                SymptomsText = "fever and cough",
                DateSubmitted = DateTime.UtcNow
            };
            var result = await service.AnalyzeSymptomsAsync(entryDto);

            // Assert: we get a non-null DTO and "headache" shows up in Summary
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.Summary));
            Assert.Contains("headache", result.Summary!, StringComparison.OrdinalIgnoreCase);
        }
    }
}
