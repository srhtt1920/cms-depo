using CMS.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CMS.Infrastructure.Persistence.Seed;

/// <summary>
/// Idempotent seed — tekrar çalıştırılabilir.
/// Root tenant + SuperAdmin'i ilk açılışta oluşturur.
/// </summary>
public static class DataSeeder
{
    // Sabit GUID'ler — migration sonrası değiştirilmemeli
    public static readonly Guid RootTenantId = new("A1000000-0000-0000-0000-000000000001");
    public static readonly Guid SuperAdminId = new("A0000000-0000-0000-0000-000000000001");
    public static readonly Guid SuperAdminRole = new("C0000000-0000-0000-0000-000000000001");

    // Development ortamı için ikincil tenant (opsiyonel)
    public static readonly Guid DevTenantId = new("B0000000-0000-0000-0000-000000000001");

    private static readonly (Guid Id, string Key, string GroupKey,
        string Display, Guid? ParentId)[] Permissions =
    [
        (new("10000000-0000-0000-0000-000000000001"), "content",            "content",  "Content Management", null),
        (new("10000000-0000-0000-0000-000000000002"), "users",              "identity", "User Management",    null),
        (new("10000000-0000-0000-0000-000000000003"), "tenants",            "tenant",   "Tenant Management",  null),
        (new("10000000-0000-0000-0000-000000000004"), "pages",              "page",     "Page Management",    null),
        (new("20000000-0000-0000-0000-000000000001"), "content.read",       "content",  "Read content",       new Guid("10000000-0000-0000-0000-000000000001")),
        (new("20000000-0000-0000-0000-000000000002"), "content.create",     "content",  "Create content",     new Guid("10000000-0000-0000-0000-000000000001")),
        (new("20000000-0000-0000-0000-000000000003"), "content.edit",       "content",  "Edit content",       new Guid("10000000-0000-0000-0000-000000000001")),
        (new("20000000-0000-0000-0000-000000000004"), "content.publish",    "content",  "Publish content",    new Guid("10000000-0000-0000-0000-000000000001")),
        (new("20000000-0000-0000-0000-000000000005"), "content.archive",    "content",  "Archive content",    new Guid("10000000-0000-0000-0000-000000000001")),
        (new("20000000-0000-0000-0000-000000000006"), "content.delete",     "content",  "Soft delete",        new Guid("10000000-0000-0000-0000-000000000001")),
        (new("20000000-0000-0000-0000-000000000007"), "content.hardDelete", "content",  "Hard delete",        new Guid("10000000-0000-0000-0000-000000000001")),
        (new("20000000-0000-0000-0000-000000000008"), "content.restore",    "content",  "Restore",            new Guid("10000000-0000-0000-0000-000000000001")),
        (new("20000000-0000-0000-0000-000000000009"), "content.detailRead", "content",  "Detail read",        new Guid("10000000-0000-0000-0000-000000000001")),
        (new("20000000-0000-0000-0000-000000000010"), "content.readDeleted","content",  "Read deleted",       new Guid("10000000-0000-0000-0000-000000000001")),
        (new("30000000-0000-0000-0000-000000000001"), "users.manage",       "identity", "Manage users",       new Guid("10000000-0000-0000-0000-000000000002")),
        (new("30000000-0000-0000-0000-000000000002"), "roles.manage",       "identity", "Manage roles",       new Guid("10000000-0000-0000-0000-000000000002")),
        (new("40000000-0000-0000-0000-000000000001"), "tenants.manage",     "tenant",   "Manage tenants",     new Guid("10000000-0000-0000-0000-000000000003")),
        (new("50000000-0000-0000-0000-000000000001"), "page.read",          "page",     "Read page",          new Guid("10000000-0000-0000-0000-000000000004")),
        (new("50000000-0000-0000-0000-000000000002"), "page.create",        "page",     "Create page",        new Guid("10000000-0000-0000-0000-000000000004")),
        (new("50000000-0000-0000-0000-000000000003"), "page.edit",          "page",     "Edit page",          new Guid("10000000-0000-0000-0000-000000000004")),
        (new("50000000-0000-0000-0000-000000000004"), "page.delete",        "page",     "Soft delete page",   new Guid("10000000-0000-0000-0000-000000000004")),
        (new("50000000-0000-0000-0000-000000000005"), "page.hardDelete",    "page",     "Hard delete page",   new Guid("10000000-0000-0000-0000-000000000004")),
        // Platform yönetim permission'ları (Root'a özgü)
        (new("60000000-0000-0000-0000-000000000001"), "platform.manage",    "platform", "Platform Management",null),
        (new("60000000-0000-0000-0000-000000000002"), "platform.audit",     "platform", "View Audit Logs",    new Guid("60000000-0000-0000-0000-000000000001")),
        (new("60000000-0000-0000-0000-000000000003"), "platform.tenants",   "platform", "Manage All Tenants", new Guid("60000000-0000-0000-0000-000000000001")),
    ];

    public static async Task SeedAsync(IServiceProvider serviceProvider, bool isDevelopment)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CmsDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CmsDbContext>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await db.Database.MigrateAsync();

        await SeedPermissionsAsync(db, logger);
        await SeedRootTenantAsync(db, logger);

        var adminPassword = Environment.GetEnvironmentVariable("CMS_ADMIN_PASSWORD");
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            if (!isDevelopment)
            {
                logger.LogError(
                    "CMS_ADMIN_PASSWORD not set. Skipping SuperAdmin seed in production.");
                return;
            }
            adminPassword = config["Seed:AdminPassword"] ?? "Dev@Password1!";
            logger.LogWarning(
                "Using development fallback password. Do NOT use in production.");
        }

        await SeedSuperAdminAsync(db, logger, adminPassword);
        await SeedSuperAdminRoleAsync(db, logger);

        // Development: isteğe bağlı test tenant
        if (isDevelopment)
            await SeedDevTenantAsync(db, logger);
    }

    // ── Root Tenant ───────────────────────────────────────────────────────

    private static async Task SeedRootTenantAsync(CmsDbContext db, ILogger logger)
    {
        var exists = await db.Tenants.IgnoreQueryFilters().AnyAsync(t => t.Id == TenantId.From(RootTenantId));
        if (exists)
        {
            logger.LogDebug("Root tenant already exists.");
            return;
        }

        // Type sütunu varsa (migration sonrası) — TenantType.Root = 1
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO Tenants (Id, Name, DefaultLanguageCode, IsActive, IsMaintenanceMode, Type, CreatedAt) " +
            "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
            RootTenantId, "System", "en", true, false, (int)TenantType.System, DateTime.UtcNow);

        await db.Database.ExecuteSqlRawAsync(
            "IF NOT EXISTS (SELECT 1 FROM SupportedLanguages WHERE TenantId = {0} AND LanguageCode = {1}) " +
            "INSERT INTO SupportedLanguages (Id, TenantId, LanguageCode, IsDefault, IsFallback) " +
            "VALUES ({2}, {0}, {1}, {3}, {3})",
            RootTenantId, "en", Guid.NewGuid(), true);

        logger.LogInformation("Root tenant seeded (Id: {Id}).", RootTenantId);
    }

    // ── SuperAdmin User ───────────────────────────────────────────────────

    private static async Task SeedSuperAdminAsync(
        CmsDbContext db, ILogger logger, string plainPassword)
    {
        if (await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Id == CMS.Domain.Identity.UserId.From(SuperAdminId)))
            return;

        var hash = BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO Users " +
            "(Id, Email, PasswordHash, IsActive, IsSuperAdmin, IsTwoFactorEnabled, " +
            " PermissionVersion, CreatedAt) " +
            "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
            SuperAdminId, "admin@gmail.com", hash,
            true, true, false, 1, DateTime.UtcNow);

        logger.LogInformation("SuperAdmin user seeded (admin@gmail.com).");
    }

    // ── SuperAdmin Role ───────────────────────────────────────────────────

    private static async Task SeedSuperAdminRoleAsync(CmsDbContext db, ILogger logger)
    {
        if (await db.Roles.IgnoreQueryFilters().AnyAsync(r => r.Id == CMS.Domain.Identity.RoleId.From(SuperAdminRole)))
            return;

         await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO Roles (Id, TenantId, Name, IsActive, CreatedAt) " +
            "VALUES ({0}, {1}, {2}, {3}, {4})",
            SuperAdminRole, RootTenantId, "SuperAdmin", true, DateTime.UtcNow);

        // Tüm permission'ları SuperAdmin'e ata
        foreach (var (permId, _, _, _, _) in Permissions)
        {
            await db.Database.ExecuteSqlRawAsync(
                "IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE RoleId = {0} AND PermissionId = {1}) " +
                "INSERT INTO RolePermissions (RoleId, PermissionId) VALUES ({0}, {1})",
                SuperAdminRole, permId);
        }

        // SuperAdmin user'a Root tenant'ta role ata
        await db.Database.ExecuteSqlRawAsync(
            "IF NOT EXISTS (SELECT 1 FROM UserTenantRoles WHERE UserId = {0} AND TenantId = {1} AND RoleId = {2}) " +
            "INSERT INTO UserTenantRoles (Id, UserId, TenantId, RoleId, AssignedAt) " +
            "VALUES ({3}, {0}, {1}, {2}, {4})",
            SuperAdminId, RootTenantId, SuperAdminRole, Guid.NewGuid(), DateTime.UtcNow);

        logger.LogInformation("SuperAdmin role seeded with {Count} permissions.", Permissions.Length);
    }

    // ── Dev Tenant (opsiyonel) ────────────────────────────────────────────

    private static async Task SeedDevTenantAsync(CmsDbContext db, ILogger logger)
    {
        var exists = await db.Tenants.IgnoreQueryFilters().AnyAsync(t => t.Id == TenantId.From(DevTenantId));
        if (exists) return;

        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO Tenants (Id, Name, DefaultLanguageCode, IsActive, IsMaintenanceMode, Type, CreatedAt) " +
            "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
            DevTenantId, "Dev Workspace", "tr", true, false, (int)TenantType.Business, DateTime.UtcNow);

        await db.Database.ExecuteSqlRawAsync(
            "IF NOT EXISTS (SELECT 1 FROM SupportedLanguages WHERE TenantId = {0} AND LanguageCode = {1}) " +
            "INSERT INTO SupportedLanguages (Id, TenantId, LanguageCode, IsDefault, IsFallback) " +
            "VALUES ({2}, {0}, {1}, {3}, {3})",
            DevTenantId, "tr", Guid.NewGuid(), true);

        // SuperAdmin'i Dev Tenant'a da ekle (dev convenience)
        await db.Database.ExecuteSqlRawAsync(
            "IF NOT EXISTS (SELECT 1 FROM UserTenantRoles WHERE UserId = {0} AND TenantId = {1} AND RoleId = {2}) " +
            "INSERT INTO UserTenantRoles (Id, UserId, TenantId, RoleId, AssignedAt) " +
            "VALUES ({3}, {0}, {1}, {2}, {4})",
            SuperAdminId, DevTenantId, SuperAdminRole, Guid.NewGuid(), DateTime.UtcNow);

        logger.LogInformation("Dev tenant seeded.");
    }

    // ── Permissions ───────────────────────────────────────────────────────

    private static async Task SeedPermissionsAsync(CmsDbContext db, ILogger logger)
    {
        if (await db.Permissions.AnyAsync()) return;

        foreach (var (id, key, groupKey, display, parentId) in Permissions)
        {
            if (parentId.HasValue)
                await db.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Permissions (Id, [Key], GroupKey, DisplayName, ParentId) " +
                    "VALUES ({0}, {1}, {2}, {3}, {4})",
                    id, key, groupKey, display, parentId.Value);
            else
                await db.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Permissions (Id, [Key], GroupKey, DisplayName, ParentId) " +
                    "VALUES ({0}, {1}, {2}, {3}, NULL)",
                    id, key, groupKey, display);
        }

        logger.LogInformation("{Count} permissions seeded.", Permissions.Length);
    }
}