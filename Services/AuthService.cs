using BCrypt.Net;
using MADAI_BACKEND.Data;
using MADAI_BACKEND.Models;
using MADAI_BACKEND.Models.DTO;
using MADAI_BACKEND.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace MADAI_BACKEND.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;


        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<SignInResponseDTO> SignIn(SignInRequestDTO signInRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == signInRequest.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(signInRequest.Password, user.PasswordHash))
            {
                return new SignInResponseDTO
                {
                    Message = "Invalid email or password"
                };
            }

            // Step 3: Correct Usage in JWT Token Generation
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new SignInResponseDTO
            {
                Token = tokenHandler.WriteToken(token)
            };
        }


        public async Task<string> Signup(SignupRequestDTO signupRequest, string creatorEmail)
        {
            // Check if email already exists
            if (_context.Users.Any(u => u.Email == signupRequest.Email))
            {
                return "Email already exists";
            }

            // Fetch the existing admin based on the provided creatorEmail
            var existingAdmin = _context.Users.FirstOrDefault(u => u.Email == creatorEmail && u.Role == UserRole.Admin);

            // Restrict admin creation to existing admins only
            if (signupRequest.Role == UserRole.Admin && existingAdmin == null)
            {
                return "Only an existing admin can create a new admin.";
            }

            // Hash the password using BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(signupRequest.Password);

            // Create a new user
            var user = new User
            {
                FirstName = signupRequest.FirstName,
                LastName = signupRequest.LastName,
                Email = signupRequest.Email,
                PasswordHash = passwordHash,
                Role = signupRequest.Role,
                CreatedAt = DateTime.UtcNow
            };

            // Add user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return "Signup successful";
        }
    }
}
