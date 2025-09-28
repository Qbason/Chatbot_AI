namespace ChatbotAIService.Services
{

    //! THIS IS THE MOST STABLE SERVICE IN THE APP, SO I CAN INJECT IT TO EVERY HANDLER/CONTROLLER. IT WOULD NOT BE THAT COUPLING

    public interface ICurrentUserService
    {
        string GetUserId();
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CurrentUserService> _logger;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public string GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.Items["UserId"]?.ToString()!;
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogCritical("UserId not found in HttpContext. Look into middleware!");
                throw new InvalidOperationException("UserId not found in HttpContext.");
            }

            return userId;
        }
    }
}