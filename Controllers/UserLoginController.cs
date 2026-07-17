using DoctorMobileApp.CommonClass;
using DoctorMobileApp.Models;
using DoctorMobileApp.WebService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace DoctorMobileApp.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserLoginController : ControllerBase
    {
        private readonly UserLoginService _loginService;
        private readonly IDbConnectionFactory _db;
        private readonly IConfiguration _configuration;
        public UserLoginController(IDbConnectionFactory db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _loginService = new UserLoginService(_db, _configuration);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            // 1. Call service
            var user = await _loginService.LoginAsync(request);
            // 2. Handle invalid login
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }
            // 3. Return success response
            return Ok(new
            {
                message = "Login successful",
                data = user
            });
        }
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(TokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest("Refresh token is required");

            var result = await _loginService.LogoutAsync(request);

            if (!result)
                return BadRequest("Logout failed");

            return Ok(new
            {
                message = "Logout successful"
            });
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest("Refresh token is required");
            var tokenData = await _loginService.GetRefreshTokenAsync(request);
            if (tokenData == null)
                return Unauthorized("Invalid or expired refresh token");
            return Ok(new
            {
                message = "Refresh Token successful",
                data = tokenData
            });

        }

    }
}
