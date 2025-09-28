using Microsoft.EntityFrameworkCore;
using ChatbotAIService.Models;
using ChatbotAIService.Services;
using ChatbotAIService.Middleware;
using System.Reflection;
using OpenAI.Chat;
using DotNetEnv;
using OpenAI;
using System.ClientModel;

Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    var originEnv = Environment.GetEnvironmentVariable("CORS_ORIGINS");
    if (string.IsNullOrEmpty(originEnv))
    {
        throw new InvalidOperationException("CORS_ORIGINS environment variable is not set.");
    }
    var origins = originEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddDbContext<ConversationContext>(options =>
    options.UseSqlServer(Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? throw new InvalidOperationException("CONNECTION_STRING environment variable not found.")));


builder.Services.AddSingleton<IStreamingConversationService, StreamingConversationService>();
builder.Services.AddScoped<IChatStreamService, ChatStreamService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

//TODO: replace with mock
builder.Services.AddSingleton<ChatClient>(sp =>
{
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    var openai_endpoint = Environment.GetEnvironmentVariable("OPENAI_API_ENDPOINT");
    var endpoint_uri = string.IsNullOrEmpty(openai_endpoint) ? null : new Uri(openai_endpoint);
    var model = Environment.GetEnvironmentVariable("OPENAI_MODEL");
    if (string.IsNullOrEmpty(apiKey))
    {
        throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");
    }
    if (endpoint_uri is null)
    {
        return new ChatClient(model, credential: new ApiKeyCredential(apiKey));
    }

    return new ChatClient(model, credential: new ApiKeyCredential(apiKey
    ), options: new OpenAIClientOptions
    {
        Endpoint = endpoint_uri
    });
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

// Enable CORS
app.UseCors("AllowAngularApp");

app.UseHttpsRedirection();
app.UseUserIdMiddleware(options =>
{
    options.SkippedPaths.Add("/api/auth");
    options.SkippedPaths.Add("/openapi");
});

app.MapControllers();


app.Run();
