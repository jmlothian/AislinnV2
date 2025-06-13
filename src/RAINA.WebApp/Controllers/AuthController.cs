using Microsoft.AspNetCore.Mvc;
using RAINA.Web.Models;
using RAINA.Web.Services;

namespace RAINA.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly string _validPassword;
        private readonly AppStateManager _appStateManager;

        public AuthController(ILogger<AuthController> logger, IConfiguration configuration, AppStateManager appStateManager)
        {
            _logger = logger;
            _appStateManager = appStateManager;  // Add this
            _validPassword = Environment.GetEnvironmentVariable("RAINA_PASSWORD")
                ?? configuration["Authentication:Password"]
                ?? "raina123";
        }

        /// <summary>
        /// Authenticate user with username and password
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Username is required",
                        Timestamp = DateTime.UtcNow
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Password is required",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Simple password validation
                if (request.Password != _validPassword)
                {
                    _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid credentials",
                        Timestamp = DateTime.UtcNow
                    });
                }
                await _appStateManager.LoginUserAsync(request.Username);

                _logger.LogInformation("Successful login for user: {Username}", request.Username);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Authentication successful",
                    Username = request.Username,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for user: {Username}", request.Username);
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "Internal server error",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Validate current session (for future use)
        /// </summary>
        [HttpPost("validate")]
        public ActionResult<AuthResponse> ValidateSession([FromBody] ValidateSessionRequest request)
        {
            try
            {
                // For now, just return success if username is provided
                // In the future, this could validate JWT tokens or session data
                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Username is required",
                        Timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Session is valid",
                    Username = request.Username,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating session for user: {Username}", request.Username);
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "Internal server error",
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}