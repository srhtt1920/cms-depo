using CMS.Application.Common.Abstractions;
using CMS.Domain.Tenants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace CMS.Infrastructure.Persistence;

public sealed class CmsDbContextFactory : IDesignTimeDbContextFactory<CmsDbContext>
{
    public CmsDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../CMS.WebApi"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connStr = config.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("SqlServer connection string not found.");

        var optionsBuilder = new DbContextOptionsBuilder<CmsDbContext>();
        optionsBuilder.UseSqlServer(connStr,
            sql => sql.MigrationsAssembly(typeof(CmsDbContext).Assembly.FullName));

        return new CmsDbContext(
            optionsBuilder.Options,
            new DesignTimeTenantContext(),
            new DesignTimePublisher(),
            NullLogger<CmsDbContext>.Instance);
    }
}

internal sealed class DesignTimeTenantContext : ITenantContext
{
    public Guid TenantId { get; } = Guid.Empty;
    public string TenantName { get; } = "design-time";
    public bool IsResolved { get; } = false;

    public TenantType TenantType => throw new NotImplementedException();

    public bool IsSystem => throw new NotImplementedException();

    public void Set(Guid tenantId, string tenantName, TenantType tenantType = TenantType.Business)
    { }
}

internal sealed class DesignTimePublisher : IPublisher
{
    public Task Publish(object notification, CancellationToken ct = default) => Task.CompletedTask;
    public Task Publish<TNotification>(TNotification notification, CancellationToken ct = default)
        where TNotification : INotification => Task.CompletedTask;
}
