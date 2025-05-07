namespace MADAI_BACKEND.Models.DTO
{
    public class DoctorDto
    {
        public string? Name { get; set; } = default!;
        public string? Address { get; set; } = default!;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PlaceId { get; set; } = default!;

        // New:
        public string? PhoneNumber { get; set; }
        public string? Website { get; set; }
        public double? Rating { get; set; }
    }
}
