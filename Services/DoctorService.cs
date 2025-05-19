using MADAI_BACKEND.Models.DTO;
using System.Text.Json;
using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Services;

namespace MADAI_BACKEND.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public DoctorService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<DoctorDto>> SearchDoctorsAsync(string location, string specialty)
        {
            var apiKey = _configuration["GoogleApiKey"];
            var query = $"{specialty} doctors in {location}";
            var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={Uri.EscapeDataString(query)}&key={apiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var results = json.GetProperty("results");

            var doctorList = new List<DoctorDto>();

            foreach (var result in results.EnumerateArray())
            {
                var placeId = result.GetProperty("place_id").GetString();
                var name = result.GetProperty("name").GetString();
                var address = result.GetProperty("formatted_address").GetString();
                var rating = result.TryGetProperty("rating", out var r) ? r.GetDouble() : 0;
                var userRatings = result.TryGetProperty("user_ratings_total", out var ur) ? ur.GetInt32() : 0;

                // Get phone & website
                var detailsUrl = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&fields=formatted_phone_number,website&key={apiKey}";
                var detailsResponse = await _httpClient.GetAsync(detailsUrl);
                detailsResponse.EnsureSuccessStatusCode();
                var detailsJson = await detailsResponse.Content.ReadFromJsonAsync<JsonElement>();

                var resultDetails = detailsJson.GetProperty("result");
                var phone = resultDetails.TryGetProperty("formatted_phone_number", out var p) ? p.GetString() ?? "" : "";
                var website = resultDetails.TryGetProperty("website", out var w) ? w.GetString() ?? "" : "";

                doctorList.Add(new DoctorDto
                {
                    Name = name ?? string.Empty,
                    Address = address ?? string.Empty,
                    PhoneNumber = phone,
                    Website = website,
                    Rating = rating,
                    UserRatingsTotal = userRatings
                });
            }

            return doctorList;
        }
    }
}
