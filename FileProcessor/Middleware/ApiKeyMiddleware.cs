using FileProcessor.Security;
using Microsoft.Extensions.Options;

namespace FileProcessor.Middleware;

public sealed class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context, IOptions<ApiOptions> options)
    {
        var path = context.Request.Path;
        if (path.StartsWithSegments("/health")
            || path.StartsWithSegments("/swagger")
            || path.StartsWithSegments("/openapi")
            || path.StartsWithSegments("/scalar"))
        {
            await _next(context);
            return;
        }
        var settings = options.Value;
        var providedKey = context.Request.Headers[settings.HeaderName].ToString();
        if (!KeysMatch(providedKey, settings.ApiKey))
        {
            _logger.LogWarning(
                "Rejected request {Method} {Path} because the API key was missing or invalid.",
                context.Request.Method,
                path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "A valid API key is required."
            });
            return;
        }
        await _next(context);
    }
    private static bool KeysMatch(string providedKey, string expectedKey)
    {
        if (string.IsNullOrEmpty(providedKey) || string.IsNullOrEmpty(expectedKey))
        {
            return false;
        }
        var providedBytes = System.Text.Encoding.UTF8.GetBytes(providedKey);
        var expectedBytes = System.Text.Encoding.UTF8.GetBytes(expectedKey);
        if (providedBytes.Length != expectedBytes.Length)
        {
            return false;
        }
        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
        providedBytes,
        expectedBytes);
    }
}