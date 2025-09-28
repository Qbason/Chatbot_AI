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

            return Ok(new
            {
                message = "Login successful",
                userId,
                authHeader = $"UserId {userId}",
                email = loginDto.Email,
                isAuthenticated = true
            });
        }
    }

    public class LoginDto
    {
        public required string Email { get; set; }
    }
}