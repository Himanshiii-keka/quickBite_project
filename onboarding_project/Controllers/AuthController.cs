using Microsoft.AspNetCore.Mvc;
using startup_project.Models;
using startup_project.Services;

namespace startup_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Register a new user. Set <c>isAdmin</c> to true for Admin, false for User. Returns a JWT on success.</summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthSuccessResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message, data) = await _authService.RegisterAsync(request);

            if (!success || data is null)
                return BadRequest(new ErrorMessageResponse { Message = message });

            return StatusCode(StatusCodes.Status201Created, new AuthSuccessResponse { Message = message, Data = data });
        }

        /// <summary>Login with email and password. Returns a JWT token on success.</summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message, data) = await _authService.LoginAsync(request);

            if (!success || data is null)
                return Unauthorized(new ErrorMessageResponse { Message = message });

            return Ok(new AuthSuccessResponse { Message = message, Data = data });
        }
    }
}
