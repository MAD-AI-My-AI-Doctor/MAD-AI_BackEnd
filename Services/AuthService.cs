using BCrypt.Net;
using MADAI_BACKEND.Data;
using MADAI_BACKEND.Models;
using MADAI_BACKEND.Models.DTO;
using MADAI_BACKEND.Contracts;
using Microsoft.EntityFrameworkCore;
using MADAI_BACKEND.Services;

namespace MADAI_BACKEND.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly Dictionary<string, string> _resetTokens = new();

        public AuthService(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<SignInResponseDTO> SignIn(SignInRequestDTO signInRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == signInRequest.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(signInRequest.Password, user.PasswordHash))
            {
                return new SignInResponseDTO
                {
                    Message = "Invalid email or password",
                    Token = string.Empty
                };
            }

            // ✅ Use JwtService to generate token
            var token = _jwtService.GenerateToken(user.Id.ToString(), user.Role.ToString());

            return new SignInResponseDTO
            {
                Token = token,
                Message = "Login successful",
                UserId = user.Id,
                Email = user.Email
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

        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            string token = Guid.NewGuid().ToString();
            var expiry = DateTime.UtcNow.AddHours(1);

            var resetToken = new PasswordResetToken
            {
                Email = email,
                Token = token,
                Expiry = expiry
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Send this reset token to user via email: {token}");
            return token;
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var resetRecord = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(r => r.Email == email && r.Token == token);

            if (resetRecord == null || resetRecord.Expiry < DateTime.UtcNow)
                return false;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.PasswordResetTokens.Remove(resetRecord);
            await _context.SaveChangesAsync();
            return true;
        }


    }
}
