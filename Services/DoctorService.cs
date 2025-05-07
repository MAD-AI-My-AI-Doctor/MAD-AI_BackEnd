using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Models.DTO;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MADAI_BACKEND.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public DoctorService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["GoogleMaps:ApiKey"]
                      ?? throw new InvalidOperationException("GoogleMaps:ApiKey is missing!");
            Console.WriteLine($"[DEBUG] Maps API Key length: {_apiKey.Length}");
        }

        public async Task<IEnumerable<DoctorDto>> SearchDoctorsAsync(string location, string specialty)
        {
            // 1) Text Search to get basic info
            var query = $"{Uri.EscapeDataString(specialty)}+in+{Uri.EscapeDataString(location)}";
            var searchUrl = $"https://maps.googleapis.com/maps/api/place/textsearch/json" +
                            $"?query={query}&type=doctor&key={_apiKey}";

            using var searchResp = await _http.GetAsync(searchUrl);
            if (!searchResp.IsSuccessStatusCode)
                throw new ApplicationException($"Places Text Search failed: {searchResp.StatusCode}");

            using var searchDoc = await JsonDocument.ParseAsync(await searchResp.Content.ReadAsStreamAsync());
            var results = new List<DoctorDto>();

            foreach (var item in searchDoc.RootElement.GetProperty("results").EnumerateArray())
            {
                var loc = item.GetProperty("geometry").GetProperty("location");
                var dto = new DoctorDto
                {
                    Name = item.GetProperty("name").GetString()!,
                    Address = item.GetProperty("formatted_address").GetString()!,
                    Latitude = loc.GetProperty("lat").GetDouble(),
                    Longitude = loc.GetProperty("lng").GetDouble(),
                    PlaceId = item.GetProperty("place_id").GetString()!
                };

                // 2) Enrich with phone & website via Place Details
                var fields = "formatted_phone_number,website";
                var detailsUrl = $"https://maps.googleapis.com/maps/api/place/details/json" +
                                 $"?place_id={dto.PlaceId}&fields={fields}&key={_apiKey}";

                using var detailsResp = await _http.GetAsync(detailsUrl);
                if (detailsResp.IsSuccessStatusCode)
                {
                    using var detailDoc = await JsonDocument.ParseAsync(await detailsResp.Content.ReadAsStreamAsync());
                    var detail = detailDoc.RootElement.GetProperty("result");

                    if (detail.TryGetProperty("formatted_phone_number", out var phone))
                        dto.PhoneNumber = phone.GetString();

                    if (detail.TryGetProperty("website", out var web))
                        dto.Website = web.GetString();
                }

                // Optional: rating is still from Text Search
                if (item.TryGetProperty("rating", out var r)) dto.Rating = r.GetDouble();

                results.Add(dto);
            }

            return results;
        }


    }
}
