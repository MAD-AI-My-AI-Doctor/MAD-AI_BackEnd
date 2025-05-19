namespace MADAI_BACKEND.Models.DTO
{
    public class ResetPasswordRequestDTO
    {
        public required string Email { get; set; }
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }
}
