using Microsoft.AspNetCore.Mvc;
using ChatbotAIService.Services;
using ChatbotAIService.Models;

//! THIS CONTROLLER IS ONLY FOR SHOW PURPOSES! TO SIMULATE AUTHENTICATION

namespace ChatbotAIService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ConversationContext _context;

        public AuthController(ConversationContext context, ICurrentUserService currentUserService)
        {

            _currentUserService = currentUserService;
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {

            var userId = _context.Users
                .Where(u => u.Email == loginDto.Email)
                .Select(u => u.Id)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid email" });
            }

            Response.Cookies.Append("userId", userId, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromDays(7),
                Path = "/"
            });

            return Ok(new
            {
                message = "Login successful",
                userId,
                email = loginDto.Email,
                isAuthenticated = true
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var currentUserId = _currentUserService.GetUserId();

            Response.Cookies.Delete("userId");

            return Ok(new
            {
                message = "Logout successful",
                previousUserId = currentUserId
            });
        }
    }

    public class LoginDto
    {
        public required string Email { get; set; }
    }
}