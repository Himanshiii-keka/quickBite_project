using System.ComponentModel.DataAnnotations;

namespace startup_project.Models
{
    // ---------- Request Models ----------

    /// <summary>Register payload. Role is derived from <see cref="IsAdmin"/> (not from a free-text role).</summary>
    public class RegisterRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;

        /// <summary>If true, the account is created as Admin; if false (default), as User.</summary>
        public bool IsAdmin { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    // ---------- Response Models ----------

    /// <summary>JWT and user profile returned after successful auth.</summary>
    public class AuthResponse
    {
        /// <summary>Bearer JWT for authenticated requests.</summary>
        public string Token { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        /// <summary>Application role name: User or Admin.</summary>
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>Successful register/login payload: human-readable message plus auth data.</summary>
    public class AuthSuccessResponse
    {
        public string Message { get; set; } = string.Empty;

        public AuthResponse Data { get; set; } = null!;
    }

    /// <summary>Simple error body for failed login or business-rule failures.</summary>
    public class ErrorMessageResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
