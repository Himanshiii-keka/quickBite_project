using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace startup_project.Common
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Reads the current user's primary key from the JWT.
        /// JwtBearer middleware maps the JWT 'sub' claim to ClaimTypes.NameIdentifier by default,
        /// but we also fall back to the raw 'sub' claim in case mapping is ever disabled.
        /// </summary>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(sub) || !int.TryParse(sub, out var id))
                throw new UnauthorizedAccessException("User identity is missing or malformed.");

            return id;
        }
    }
}
