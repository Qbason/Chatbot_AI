using Microsoft.EntityFrameworkCore;
using ChatbotAIService.Models;
using ChatbotAIService.Services;
using ChatbotAIService.Middleware;
using System.Reflection;
using OpenAI.Chat;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ConversationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ChatbotContext") ?? throw new InvalidOperationException("Connection string 'ChatbotContext' not found.")));


builder.Services.AddSingleton<IStreamingConversationService, StreamingConversationService>();
builder.Services.AddScoped<IChatStreamService, ChatStreamService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

//TODO: replace with mock
builder.Services.AddSingleton<ChatClient>(sp =>
{
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    var model = "gpt-4o";
    if (string.IsNullOrEmpty(apiKey))
    {
        throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");
    }
    return new ChatClient(model, apiKey);
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
{
    options.DocumentPath = "/openapi/v1.json";
});
}

app.UseHttpsRedirection();
app.UseUserIdMiddleware(options => 
{
    options.SkippedPaths.Add("/api/auth");
    options.SkippedPaths.Add("/openapi");
});

app.MapControllers();


app.Run();
