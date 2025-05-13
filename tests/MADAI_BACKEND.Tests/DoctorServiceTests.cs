using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;
using MADAI_BACKEND.Models.DTO;
using MADAI_BACKEND.Services;
using MADAI_BACKEND.Tests.Helpers;

namespace MADAI_BACKEND.Tests
{
    public class DoctorServiceTests
    {
        [Fact]
        public async Task SearchDoctorsAsync_ParsesTextSearchAndDetailsCorrectly()
        {
            // 1) Text search JSON
            var textSearchJson = JsonSerializer.Serialize(new
            {
                results = new[]
                {
                    new {
                        name = "Dr. Test",
                        formatted_address = "123 Test St",
                        geometry = new { location = new { lat = 1.23, lng = 4.56 } },
                        place_id = "place-123",
                        rating = 4.7
                    }
                }
            });

            // 2) Details JSON
            var detailsJson = JsonSerializer.Serialize(new
            {
                result = new
                {
                    formatted_phone_number = "555-1234",
                    website = "https://test.example.com"
                }
            });

            var handler = new FakeHttpMessageHandler(req =>
            {
                if (req.RequestUri.AbsoluteUri.Contains("/textsearch/"))
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(textSearchJson, Encoding.UTF8, "application/json")
                    };
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(detailsJson, Encoding.UTF8, "application/json")
                };
            });

            var httpClient = new HttpClient(handler);
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string,string>("GoogleMaps:ApiKey","fake-key")
                })
                .Build();

            var svc = new DoctorService(httpClient, config);

            // Act
            var results = (await svc.SearchDoctorsAsync("1000", "cardiologist")).ToList();

            // Assert
            Assert.Single(results);
            var doc = results[0];
            Assert.Equal("Dr. Test", doc.Name);
            Assert.Equal("123 Test St", doc.Address);
            Assert.Equal(1.23, doc.Latitude);
            Assert.Equal(4.56, doc.Longitude);
            Assert.Equal("place-123", doc.PlaceId);
            Assert.Equal("555-1234", doc.PhoneNumber);
            Assert.Equal("https://test.example.com", doc.Website);
            Assert.Equal(4.7, doc.Rating);
        }
    }
}
