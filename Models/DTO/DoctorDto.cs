namespace MADAI_BACKEND.Models.DTO
{
    public class DoctorDto
    {

        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int UserRatingsTotal { get; set; }
    }
}
