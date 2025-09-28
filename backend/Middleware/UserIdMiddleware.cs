namespace ChatbotAIService.Middleware
{
    public class UserIdMiddlewareOptions
    {
        public List<string> SkippedPaths { get; set; } = new()
        {
            "/api/auth",
            "/openapi",
            "/swagger"
        };
    }

    public class UserIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly UserIdMiddlewareOptions _options;
        private const string USER_ID_COOKIE_NAME = "userId";

        public UserIdMiddleware(RequestDelegate next, UserIdMiddlewareOptions? options = null)
        {
            _next = next;
            _options = options ?? new UserIdMiddlewareOptions();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (ShouldSkipMiddleware(context))
            {
                await _next(context);
                return;
            }

            string? userId = context.Request.Cookies[USER_ID_COOKIE_NAME];
            if(string.IsNullOrEmpty(userId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: No userId cookie found");
                return;
            }

            context.Items["UserId"] = userId;

            await _next(context);
        }
        private bool ShouldSkipMiddleware(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant();
            
            if (path == null) return false;

            return _options.SkippedPaths.Any(skipPath => 
                path.StartsWith(skipPath, StringComparison.InvariantCultureIgnoreCase));
        }
    }

    public static class UserIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserIdMiddleware(this IApplicationBuilder builder, Action<UserIdMiddlewareOptions> configureOptions)
        {
            var options = new UserIdMiddlewareOptions();
            configureOptions(options);
            return builder.UseMiddleware<UserIdMiddleware>(options);
        }
    }
}