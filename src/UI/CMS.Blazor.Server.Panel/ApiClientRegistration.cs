using CMS.Blazor.Server.Panel.Infrastructure.ApiClients.Auth;
using CMS.Blazor.Server.Panel.Infrastructure.ApiClients.Identity;
using CMS.Blazor.Server.Panel.Infrastructure.ApiClients.PageContent;
using CMS.Blazor.Server.Panel.Infrastructure.ApiClients.Tenant;
namespace CMS.Blazor.Server.Panel;

/// <summary>
/// Tüm API istemcilerini DI konteynerine kaydeder.
/// Program.cs içinde builder.Services.AddCmsApiClients(); yeterlidir.
/// </summary>
public static class ApiClientRegistration
{
    public static IServiceCollection AddCmsApiClients(this IServiceCollection services)
    {
        services.AddScoped<AuthApiClient>();
        services.AddScoped<ContentApiClient>();
        services.AddScoped<PageApiClient>();
        services.AddScoped<TenantApiClient>();
        services.AddScoped<UserApiClient>();
        services.AddScoped<RoleApiClient>();
        services.AddScoped<ApprovalApiClient>();
        services.AddScoped<SuperAdminApiClient>();
        services.AddScoped<DashboardApiClient>();
        services.AddScoped<AuditApiClient>();
        services.AddScoped<SeoApiClient>();
        services.AddScoped<InviteApiClient>();
        services.AddScoped<TenantProvisioningApiClient>();
        return services;
    }
}
