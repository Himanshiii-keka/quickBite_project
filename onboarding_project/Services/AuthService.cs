using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using startup_project.Data;
using startup_project.Models;
using startup_project.Models.Enums;

namespace startup_project.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        public AuthService(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<(bool Success, string Message, AuthResponse? Data)> RegisterAsync(RegisterRequest request)
        {
            // Check if email or phone already taken
            bool emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email);
            if (emailExists)
                return (false, "A user with this email already exists. Please log in.", null);

            bool phoneExists = await _db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber);
            if (phoneExists)
                return (false, "A user with this phone number already exists. Please log in.", null);

            var user = new User
            {
                Name = request.Name,
                Email = request.Email.ToLower().Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.IsAdmin ? UserRole.Admin : UserRole.User,
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            return (true, "Registration successful.", new AuthResponse
            {
                Token = token,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString()
            });
        }

        public async Task<(bool Success, string Message, AuthResponse? Data)> LoginAsync(LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim());

            if (user == null)
                return (false, "Invalid email or password.", null);

            bool passwordMatch = BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword);
            if (!passwordMatch)
                return (false, "Invalid email or password.", null);

            var token = GenerateJwtToken(user);
            return (true, "Login successful.", new AuthResponse
            {
                Token = token,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString()
            });
        }

        // Builds a JWT token with user identity claims
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"]!));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
