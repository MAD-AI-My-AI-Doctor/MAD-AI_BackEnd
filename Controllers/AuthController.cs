using MADAI_BACKEND.Contracts;
using MADAI_BACKEND.Models.DTO;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequestDTO signupRequest)
        {
            // For simplicity, assuming the creator role is Admin if the user is logged in as Admin
            string creatorRole = User.Identity?.IsAuthenticated == true ? "Admin" : "Patient";

            var result = await _authService.Signup(signupRequest, creatorRole);

            if (result == "Signup successful")
            {
                return Ok(new { message = result });
            }
            return BadRequest(new { error = result });
        }
    }
}