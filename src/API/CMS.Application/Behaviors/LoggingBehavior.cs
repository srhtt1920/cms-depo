using MediatR;
using Microsoft.Extensions.Logging;

namespace CMS.Application.Behaviors;

/// <summary>
/// Request/response loglar. Hassas alanlar (Password, Token, Hash) maskelenir.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // Değeri loglanmaması gereken property isimleri (küçük harf)
    private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "passwordhash", "hash", "token", "secret", "apikey", "connectionstring"
    };

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Handling {RequestName}", requestName);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            var safe = MaskSensitive(request);
            logger.LogDebug("Request payload: {RequestName} {@Request}", requestName, safe);
        }

        var response = await next();
        logger.LogInformation("Handled {RequestName}", requestName);
        return response;
    }

    private static object MaskSensitive(TRequest request)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in typeof(TRequest).GetProperties())
        {
            if (SensitiveFields.Contains(prop.Name))
                dict[prop.Name] = "***";
            else
                dict[prop.Name] = prop.GetValue(request);
        }
        return dict;
    }
}
