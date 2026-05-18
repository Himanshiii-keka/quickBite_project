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
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(request);

            if (!result.Success || result.Data is null)
                return StatusCode(result.StatusCode, new ErrorMessageResponse { Message = result.Message });

            return StatusCode(result.StatusCode, new AuthSuccessResponse { Message = result.Message, Data = result.Data });
        }

        /// <summary>Login with email and password. Returns a JWT token on success.</summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(request);

            if (!result.Success || result.Data is null)
                return StatusCode(result.StatusCode, new ErrorMessageResponse { Message = result.Message });

            return Ok(new AuthSuccessResponse { Message = result.Message, Data = result.Data });
        }
    }
}
