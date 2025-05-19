using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MADAI_BACKEND.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequestDTO signInRequest)
        {
            var response = await _authService.SignIn(signInRequest);
            if (response.Token == string.Empty)
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequestDTO signupRequest)
        {
            // Get the email of the authenticated user, if any
            string creatorEmail = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue(ClaimTypes.Email) ?? ""
                : "";

            var result = await _authService.Signup(signupRequest, creatorEmail);

            if (result == "Signup successful")
            {
                return Ok(new { message = result });
            }
            return BadRequest(new { error = result });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
        {
            var token = await _authService.ForgotPasswordAsync(request.Email);
            if (token == null)
                return NotFound("User not found.");

            return Ok(new { Message = "Reset token generated. Check your email.", Token = token });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            var success = await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
            if (!success)
                return BadRequest("Invalid or expired token.");

            return Ok("Password reset successfully.");
        }


    }




}
