using CMS.Application.Common.Abstractions;
using CMS.Domain.Common;
using CMS.Domain.Contents;
using CMS.Domain.Contents.Versioning;
using CMS.Domain.Identity;
using CMS.Domain.Pages;
using CMS.Domain.Tenants;
using CMS.Infrastructure.BackgroundJobs;
using CMS.Infrastructure.Caching;
using CMS.Infrastructure.Identity;
using CMS.Infrastructure.Persistence;
using CMS.Infrastructure.Persistence.Repositories;
using CMS.Infrastructure.Publishing;
using CMS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace CMS.WebApi.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // ── Context'ler — Scoped (request başına bir instance) ───────────────
        // TenantContext: TenantResolutionBehavior tarafından doldurulur,
        // CmsDbContext ve tüm handler'lar tarafından okunur.
        // AddScoped olmalı — AddSingleton olursa tenant bilgisi requestlar arası sızar!
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<ILanguageContext, LanguageContext>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();


        // Database — CmsDbContext artık ITenantContext'i scope'tan alabilir
        services.AddDbContext<CmsDbContext>(opts =>
            opts.UseSqlServer(
                config.GetConnectionString("SqlServer"),
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3)));

        // Repositories
        services.AddScoped<IContentRepository, ContentRepository>();
        services.AddScoped<IPageRepository, PageRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IContentVersionRepository, ContentVersionRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IContentLockRepository, ContentLockRepository>();

        // ── Cache ─────────────────────────────────────────────────────────────
        var cacheProvider = config["Cache:Provider"] ?? "Memory";
        if (cacheProvider == "Redis")
        {
            services.AddSingleton<IConnectionMultiplexer>(
                _ => ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!));
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }

        // Servisler
        services.AddSingleton<ITotpService, TotpService>();
        services.AddSingleton<IEmailSender, EmailSender>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPermissionService, PermissionCacheService>();
        services.AddScoped<ILanguageResolver, LanguageCacheService>();
        services.AddScoped<ITokenBlacklist, TokenBlacklist>();
        services.AddScoped<ITenantPolicyService, TenantPolicyService>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<IUserTenantAssignmentPolicy, UserTenantAssignmentPolicy>();
        services.AddSingleton<ICmsHtmlSanitizer, CmsHtmlSanitizer>();

        // Publish pipeline
        services.AddScoped<IPublishPipelineHandler, SitemapRefreshPipelineHandler>();
        services.AddScoped<IPublishPipeline, CompositePublishPipeline>();

        // Background jobs
        services.AddHostedService<ContentScheduleWatcher>();

        return services;
    }
}
