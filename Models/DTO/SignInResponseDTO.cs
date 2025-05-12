namespace MADAI_BACKEND.Models.DTO
{
    public class SignInResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = "Sign in successful";
    }
}
