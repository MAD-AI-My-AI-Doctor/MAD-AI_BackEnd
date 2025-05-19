using MADAI_BACKEND.Models.DTO;

namespace MADAI_BACKEND.Contracts
{
    public interface IAuthService
    {
        Task<string> Signup(SignupRequestDTO signupRequest, string creatorEmail);
        Task<SignInResponseDTO> SignIn(SignInRequestDTO signInRequest);

        Task<string> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    }
}
