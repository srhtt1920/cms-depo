using CMS.Application.Common.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CMS.Application.Behaviors;

public sealed class LanguageBehavior<TRequest, TResponse>(
    ILanguageContext languageContext,
    ITenantContext tenantContext,
    IHttpContextAccessor http,
    ILanguageResolver languageResolver)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requested = http.HttpContext?
            .Request.Headers["Accept-Language"]
            .FirstOrDefault() ?? "en";

        requested = requested.Split('-', ';')[0].ToLowerInvariant().Trim();

        var (fallback, @default) = tenantContext.IsResolved
            ? await languageResolver.ResolveAsync(tenantContext.TenantId, ct)
            : ("en", "en");

        languageContext.Set(requested, fallback, @default);

        return await next();
    }
}
