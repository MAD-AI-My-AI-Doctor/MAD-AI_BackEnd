using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using MADAI_BACKEND.Services;
using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Models.DTO;
using MADAI_BACKEND.Tests.Helpers;

namespace MADAI_BACKEND.Tests
{
    public class RecommendationServiceTests
    {
        [Fact]
        public async Task RecommendDoctorsByDiseaseAsync_UsesAIForSpecialtyThenSearches()
        {
            // 1) Fake AI response: returns "dermatologist"
            var chatJson = JsonSerializer.Serialize(new
            {
                choices = new[]
                {
                    new { message = new { content = "dermatologist" } }
                }
            });

            var handler = new FakeHttpMessageHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(chatJson, Encoding.UTF8, "application/json")
                });
            var httpClient = new HttpClient(handler);

            // 2) Mock the DoctorService
            var expectedDocs = new List<DoctorDto>
            {
                new DoctorDto { Name="Skin Expert", Address="Derm St", Latitude=0, Longitude=0, PlaceId="p1" }
            };
            var doctorMock = new Mock<IDoctorService>();
            doctorMock
                .Setup(d => d.SearchDoctorsAsync("Copenhagen", "dermatologist"))
                .ReturnsAsync(expectedDocs);

            // 3) In‐memory config
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string,string>("OpenRouter:ApiKey","fake-key")
                })
                .Build();

            var svc = new RecommendationService(httpClient, doctorMock.Object, config);

            // Act
            var actual = await svc.RecommendDoctorsByDiseaseAsync("eczema", "Copenhagen");

            // Assert
            Assert.Same(expectedDocs, actual);
            doctorMock.Verify(d => d.SearchDoctorsAsync("Copenhagen", "dermatologist"), Times.Once);
        }
    }
}
