using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MADAI_BACKEND.Models
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        Admin,
        Patient
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Patient;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
