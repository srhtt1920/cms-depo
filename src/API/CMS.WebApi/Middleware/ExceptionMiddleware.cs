using CMS.SharedKernel.Exceptions;
using System.Text.Json;

namespace CMS.WebApi.Middleware;

/// <summary>
/// Global exception handler.
/// Domain akışındaki hatalar Result pattern ile yönetilir — buraya düşmez.
/// Sadece infrastructure seviyesi gerçek exception'lar buraya gelir.
/// </summary>
public sealed class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger)
{
    // Tip reflection tetiklememek için anonymous object kullan
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AuthorizationException ex)
        {
            logger.LogWarning(ex, "Authorization failure: {Path}", context.Request.Path);
            await WriteProblemAsync(context,
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: ex.Message,
                type: "https://tools.ietf.org/html/rfc7235#section-3.1");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Path}", context.Request.Path);

            var isDev = context.RequestServices
                .GetRequiredService<IHostEnvironment>()
                .IsDevelopment();

            await WriteProblemAsync(context,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal server error",
                detail: isDev ? ex.Message : "An unexpected error occurred.",
                type: "https://tools.ietf.org/html/rfc7231#section-6.6.1");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        string type)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        // Anonymous object — JsonSerializer tip metadata okumaz, reflection yok
        var body = new
        {
            type,
            title,
            status = statusCode,
            detail
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(body, JsonOpts));
    }
}
