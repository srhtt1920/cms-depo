using CMS.Application.Common.Abstractions;
using CMS.Domain.Common;
using CMS.Domain.Contents;
using CMS.Domain.Identity;
using CMS.Domain.Pages;
using CMS.Domain.Tenants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection.Emit;

namespace CMS.Infrastructure.Persistence;

public sealed class CmsDbContext(
    DbContextOptions<CmsDbContext> options,
    ITenantContext tenantContext,
    IPublisher publisher,
    ILogger<CmsDbContext> logger)
    : DbContext(options)
{
    public DbSet<AuditLog>           AuditLogs           => Set<AuditLog>();
    public DbSet<Tenant>             Tenants             => Set<Tenant>();
    public DbSet<SupportedLanguage>  SupportedLanguages  => Set<SupportedLanguage>();
    public DbSet<Content>            Contents            => Set<Content>();
    public DbSet<ContentTranslation> ContentTranslations => Set<ContentTranslation>();
    public DbSet<ContentSection>     ContentSections     => Set<ContentSection>();
    public DbSet<ContentBlock>       ContentBlocks       => Set<ContentBlock>();
    public DbSet<BlockTranslation>   BlockTranslations   => Set<BlockTranslation>();
    public DbSet<User>               Users               => Set<User>();
    public DbSet<Role>               Roles               => Set<Role>();
    public DbSet<Permission>         Permissions         => Set<Permission>();
    public DbSet<UserTenantRole>     UserTenantRoles     => Set<UserTenantRole>();
    public DbSet<RefreshToken>       RefreshTokens       => Set<RefreshToken>();
    public DbSet<Page>               Pages               => Set<Page>();
    public DbSet<PageTranslation>    PageTranslations    => Set<PageTranslation>();
    public DbSet<ApprovalInfo>       ApprovalInfos       => Set<ApprovalInfo>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfigurationsFromAssembly(typeof(CmsDbContext).Assembly);
        mb.Entity<Content>()
            .HasQueryFilter(c => c.TenantId == tenantContext.TenantId);
        mb.Entity<Role>()
            .HasQueryFilter(r => r.TenantId == TenantId.From(tenantContext.TenantId));
        base.OnModelCreating(mb);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var aggregates = ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var events = aggregates.SelectMany(a => a.DomainEvents).ToList();
        foreach (var a in aggregates) a.ClearDomainEvents();

        var result = await base.SaveChangesAsync(ct);

        // TODO: Outbox pattern ile değiştir
        foreach (var ev in events)
        {
            try   { await publisher.Publish(ev, ct); }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Domain event publish failed for {EventType}.", ev.GetType().Name);
            }
        }
        return result;
    }
}
