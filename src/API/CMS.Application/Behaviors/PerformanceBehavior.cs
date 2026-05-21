using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CMS.Application.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int WarningThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        if (sw.ElapsedMilliseconds > WarningThresholdMs)
        {
            logger.LogWarning(
                "Slow request detected: {RequestName} took {ElapsedMs}ms. {@Request}",
                typeof(TRequest).Name,
                sw.ElapsedMilliseconds,
                request);
        }

        return response;
    }
}
