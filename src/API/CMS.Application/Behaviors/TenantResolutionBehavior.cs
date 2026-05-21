using CMS.Application.Common.Abstractions;
using CMS.Domain.Tenants;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CMS.Application.Behaviors;

/// <summary>
/// Her MediatR pipeline'ýnda TenantId'yi çözer ve ITenantContext'e set eder.
///
/// Öncelik sýrasý:
///   1. X-Tenant-Id header (Public/Headless API)
///   2. JWT claim "tenantId"  (Panel — select-tenant sonrasý token'da bulunur)
///
/// DB çaðrýsý YAPILMAZ — sadece header veya JWT claim okunur.
/// Tenant adý JWT'deki "tenantName" claim'inden alýnýr.
/// Böylece her request'e ekstra DB sorgusu eklenmez ve
/// TenantId dolmadan önce DbContext kullanýlmasý engellenir.
/// </summary>
public sealed class TenantResolutionBehavior<TRequest, TResponse>(
    ITenantContext tenantContext,
    IHttpContextAccessor http,
    ILogger<TenantResolutionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const string TenantHeader = "X-Tenant-Id";
    private const string TenantClaim = "tenantId";
    private const string TenantNameClaim = "tenantName";
    private const string TenantTypeClaim = "tenantType";

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (tenantContext.IsResolved)
            return await next(ct);

        var httpContext = http.HttpContext;
        if (httpContext is null)
            return await next(ct);

        Guid tenantId = Guid.Empty;
        string tenantName = string.Empty;

        // 1. Header — Public API (X-Tenant-Id)
        if (httpContext.Request.Headers.TryGetValue(TenantHeader, out var headerValue)
            && Guid.TryParse(headerValue, out var headerTenantId))
        {
            tenantId = headerTenantId;
            tenantName = string.Empty; // Public API tenant adý bilinmiyor, sadece ID yeterli
        }
        // 2. JWT claim — Panel (select-tenant sonrasý token)
        else
        {
            var idClaim = httpContext.User.FindFirst(TenantClaim)?.Value;
            var nameClaim = httpContext.User.FindFirst(TenantNameClaim)?.Value;

            if (!string.IsNullOrEmpty(idClaim) && Guid.TryParse(idClaim, out var claimTenantId))
            {
                tenantId = claimTenantId;
                tenantName = nameClaim ?? string.Empty;
            }
        }

        if (tenantId == Guid.Empty)
        {
            logger.LogDebug("TenantId çözümlenemedi — {Request}", typeof(TRequest).Name);
            return await next(ct);
        }

        var tenantTypeRaw = httpContext.User.FindFirst(TenantTypeClaim)?.Value;
        var tenantType = Enum.TryParse<TenantType>(tenantTypeRaw, out var tt)
            ? tt
            : TenantType.Business;

        tenantContext.Set(tenantId, tenantName, tenantType);
        logger.LogDebug("TenantId çözümlendi: {TenantId}", tenantId);

        return await next(ct);
    }
}
