using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Meteohost.MiddleWare;

public class ApiKeyMiddleWare
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyMiddleWare> _logger;
    private const string ApiKeyHeaderName = "X-Api-Key";

    public ApiKeyMiddleWare(
        RequestDelegate next, 
        IConfiguration configuration,
        ILogger<ApiKeyMiddleWare> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        
        if (IsPublicRequest(context))
        {
            _logger.LogDebug("Public request: {Path}", path);
            await _next(context);
            return;
        }

        string? apiKey = null;
        
        if (context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var headerKey))
        {
            apiKey = headerKey.ToString();
            _logger.LogDebug("Key from header: {Key}", apiKey);
        }
        
        if (string.IsNullOrEmpty(apiKey) && context.Request.Query.TryGetValue("apiKey", out var queryKey))
        {
            apiKey = queryKey.ToString();
            _logger.LogDebug("Key from query: {Key}", apiKey);
        }

        if (string.IsNullOrEmpty(apiKey) && context.Request.HasJsonContentType())
        {
            try
            {
                context.Request.EnableBuffering();
                var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;
                
                if (!string.IsNullOrEmpty(body))
                {
                    var json = System.Text.Json.JsonDocument.Parse(body);
                    if (json.RootElement.TryGetProperty("apiKey", out var jsonKey))
                    {
                        apiKey = jsonKey.GetString();
                        _logger.LogDebug("Key from body: {Key}", apiKey);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to read body for API key: {Error}", ex.Message);
            }
        }

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Missing API key for: {Path}", path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "X-Api-Key пропущен",
                errorCode = "MISSING_API_KEY",
                message = "Укажите X-Api-Key в заголовке запроса",
                timestamp = DateTime.UtcNow
            });
            return;
        }

        var expectedKey = _configuration["ApiKey"];
        _logger.LogDebug("Expected: {Expected}, Received: {Received}", expectedKey, apiKey);

        if (string.IsNullOrEmpty(expectedKey) || !expectedKey.Equals(apiKey))
        {
            _logger.LogWarning("Invalid API key for: {Path}", path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Неправильный API ключ",
                errorCode = "INVALID_API_KEY",
                message = "Проверьте правильность X-Api-Key",
                timestamp = DateTime.UtcNow
            });
            return;
        }


        _logger.LogInformation("✅ Валидный API ключ для: {Path}", path);
        await _next(context);
    }

    private bool IsPublicRequest(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        return path.StartsWith("/swagger") ||
            path.StartsWith("/health") ||
            path == "/" ||
            path.StartsWith("/info") ||
            path.StartsWith("/api/webhook/express") ||
            path.StartsWith("/api/Proof/ping") ||
            path.StartsWith("/api/Proof/info") ||
            path.StartsWith("/api/Proof/chats");
    }
}
