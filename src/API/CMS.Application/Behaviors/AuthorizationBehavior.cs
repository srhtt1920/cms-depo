using CMS.Application.Common.Abstractions;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse>(
    ICurrentUser currentUser,
    ITenantContext tenantContext,
    IPermissionService permissionService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var attributes = typeof(TRequest)
            .GetCustomAttributes(typeof(RequirePermissionAttribute), inherit: true)
            .Cast<RequirePermissionAttribute>()
            .ToList();

        if (attributes.Count == 0)
            return await next();

        if (!currentUser.IsAuthenticated)
            return Result.CreateFailure<TResponse>(
                Error.Unauthorized("Auth.Required", "Authentication required."));

        foreach (var attr in attributes)
        {
            var hasPermission = await permissionService.HasPermissionAsync(
                currentUser.UserId,
                tenantContext.TenantId,
                attr.PermissionKey,
                ct);

            if (!hasPermission)
                return Result.CreateFailure<TResponse>(
                    Error.Forbidden("Auth.Forbidden", $"Missing permission: {attr.PermissionKey}"));
        }

        return await next();
    }
}
