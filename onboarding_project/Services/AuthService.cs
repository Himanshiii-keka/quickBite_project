using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using startup_project.Data;
using startup_project.Models;
using startup_project.Models.Common;
using startup_project.Models.Enums;

namespace startup_project.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationDbContext db, IConfiguration config, ILogger<AuthService> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request)
        {
            try
            {
                bool emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email.ToLower().Trim());
                if (emailExists)
                    return ServiceResult<AuthResponse>.Fail(StatusCodes.Status400BadRequest,
                        "A user with this email already exists. Please log in.");

                bool phoneExists = await _db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber.Trim());
                if (phoneExists)
                    return ServiceResult<AuthResponse>.Fail(StatusCodes.Status400BadRequest,
                        "A user with this phone number already exists. Please log in.");

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
                return ServiceResult<AuthResponse>.Created(new AuthResponse
                {
                    Token = token,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString()
                }, "Registration successful.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error during registration for email {Email}", request.Email);
                return ServiceResult<AuthResponse>.Fail(StatusCodes.Status500InternalServerError,
                    "Registration failed due to a database error.");
            }
        }

        public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim());

                if (user == null)
                    return ServiceResult<AuthResponse>.Fail(StatusCodes.Status401Unauthorized, "Invalid email or password.");

                bool passwordMatch = BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword);
                if (!passwordMatch)
                    return ServiceResult<AuthResponse>.Fail(StatusCodes.Status401Unauthorized, "Invalid email or password.");

                var token = GenerateJwtToken(user);
                return ServiceResult<AuthResponse>.Ok(new AuthResponse
                {
                    Token = token,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString()
                }, "Login successful.");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Unexpected error during login for email {Email}", request.Email);
                return ServiceResult<AuthResponse>.Fail(StatusCodes.Status500InternalServerError,
                    "Login failed due to an unexpected error.");
            }
        }

        private string GenerateJwtToken(User user)
        {
            // Encrypt userId into a token signed with the secret key.
            // To use the token: decrypt it with the same key → read userId from "sub".
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiryMinutes = double.TryParse(_config["Jwt:ExpiryMinutes"], out var mins) ? mins : 60;

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),  // userId goes here
                new Claim(ClaimTypes.Role, user.Role.ToString())             // role for [Authorize(Roles=...)]
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
