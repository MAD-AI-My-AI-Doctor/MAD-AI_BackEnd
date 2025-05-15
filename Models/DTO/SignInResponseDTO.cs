namespace MADAI_BACKEND.Models.DTO
{
    public class SignInResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = "Sign in successful";

        public int UserId { get; set; }

        // ✅ Made Email nullable to avoid CS9035 error
        public string? Email { get; set; }

    }
}
