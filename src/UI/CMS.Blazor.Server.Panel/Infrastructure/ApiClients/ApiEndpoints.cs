namespace CMS.Blazor.Server.Panel.Infrastructure.ApiClients;

/// <summary>
/// Tüm API endpoint path'leri tek yerden yönetilir.
/// Endpoint değişirse yalnızca bu dosyayı güncellemek yeterlidir.
/// </summary>
public static class ApiEndpoints
{
    // ── Auth ──────────────────────────────────────────────────────────────
    public static class Auth
    {
        public const string Login = "api/auth/login";
        public const string Refresh = "api/auth/refresh";
        public const string Logout = "api/auth/logout";
        public const string SelectTenant = "api/auth/switch-tenant";
        public const string Me = "api/auth/me";
        public const string MePassword = "api/auth/me/password";
        public const string ForgotPassword = "api/auth/forgot-password";
        public const string ResetPassword = "api/auth/reset-password";
        public const string TwoFactorSetup = "api/auth/2fa/setup";
        public const string TwoFactorVerify = "api/auth/2fa/verify";
        public const string TwoFactorDisable = "api/auth/2fa/disable";
        public const string TwoFactorResend = "api/auth/2fa/resend";
        public const string MyPermissions = "api/me/permissions";
    }

    // ── Contents ──────────────────────────────────────────────────────────
    public static class Contents
    {
        public const string Base = "api/contents";
        public const string Trash = "api/contents/trash";
        public const string Bulk = "api/contents/bulk";
        public const string BulkLock = "api/contents/bulk-lock";

        public static string ById(Guid id) => $"api/contents/by-id/{id}";
        public static string BySlug(string slug) => $"api/contents/{slug}";
        public static string Update(Guid id) => $"api/contents/{id}";
        public static string Publish(Guid id) => $"api/contents/{id}/publish";
        public static string Unpublish(Guid id) => $"api/contents/{id}/unpublish";
        public static string Archive(Guid id) => $"api/contents/{id}/archive";
        public static string SoftDelete(Guid id) => $"api/contents/{id}";
        public static string Duplicate(Guid id) => $"api/contents/{id}/duplicate";
        public static string Translations(Guid id) => $"api/contents/{id}/translations";
        public static string Lock(Guid id) => $"api/contents/{id}/lock";
        public static string Schedule(Guid id) => $"api/contents/{id}/schedule";
        public static string Sections(Guid id) => $"api/contents/{id}/sections";
        public static string Versions(Guid id) => $"api/contents/{id}/versions";
        public static string RestoreVersion(Guid id, int version) => $"api/contents/{id}/versions/{version}/restore";

        // Completion
        public static string Completion(Guid id) => $"api/contents/{id}/completion";

        // Trash sub-routes
        public static string TrashHardDelete(Guid id) => $"api/contents/trash/{id}/hard";
        public static string TrashRestore(Guid id) => $"api/contents/trash/{id}/restore";
        public static string TrashCompletion(Guid id) => $"api/contents/trash/{id}/completion";

        // Sections
        public static string Section(Guid contentId, Guid sectionId) => $"api/contents/{contentId}/sections/{sectionId}";
        public static string SectionsReorder(Guid contentId) => $"api/contents/{contentId}/sections/reorder";

        // Blocks
        public static string Blocks(Guid contentId, Guid sectionId) => $"api/contents/{contentId}/sections/{sectionId}/blocks";
        public static string Block(Guid contentId, Guid sectionId, Guid blockId) => $"api/contents/{contentId}/sections/{sectionId}/blocks/{blockId}";
        public static string BlockTranslations(Guid contentId, Guid sectionId, Guid blockId) => $"api/contents/{contentId}/sections/{sectionId}/blocks/{blockId}/translations";
    }

    // ── Pages ──────────────────────────────────────────────────────────────
    public static class Pages
    {
        public const string Base = "api/pages";
        public const string Tree = "api/pages/tree";
        public const string TreeDetail = "api/pages/tree/detail";

        public static string ById(Guid id) => $"api/pages/{id}";
        public static string Delete(Guid id) => $"api/pages/{id}";
        public static string Translations(Guid id) => $"api/pages/{id}/translations";
        public static string Move(Guid id) => $"api/pages/{id}/move";
        public static string ContentLink(Guid id) => $"api/pages/{id}/content-link";
        public static string Active(Guid id) => $"api/pages/{id}/active";
        public static string Settings(Guid id) => $"api/pages/{id}";
        public static string Lock(Guid id) => $"api/pages/{id}/lock";
        public static string Breadcrumb(Guid id) => $"api/pages/{id}/breadcrumb";
    }

    // ── Users ──────────────────────────────────────────────────────────────
    public static class Users
    {
        public const string Base = "api/users";

        public static string ById(Guid id) => $"api/users/{id}";
        public static string Roles(Guid id) => $"api/users/{id}/roles";
        public static string Active(Guid id) => $"api/users/{id}/active";
        public static string Delete(Guid id) => $"api/users/{id}";
        public static string Activity(Guid id) => $"api/users/{id}/activity";

        // ── Invite ─────────────────────────────────────────────────────────
        public const string Invite = "api/users/invite";
        public const string Invites = "api/users/invites";
        public static string InviteById(Guid id) => $"api/users/invites/{id}";
        public static string InviteAccept(string token) => $"api/users/invites/{token}/accept";
        public static string InviteReject(string token) => $"api/users/invites/{token}/reject";
        public static string InviteResend(Guid id) => $"api/users/invites/{id}/resend";
        public static string InviteCancel(Guid id) => $"api/users/invites/{id}/cancel";

        // ── User-Tenant Membership ─────────────────────────────────────────
        public static string UserTenants(Guid userId) => $"api/users/{userId}/tenants";
        public static string MembershipStatus(Guid userId, Guid tenantId)
            => $"api/users/{userId}/tenants/{tenantId}/status";
    }

    // ── Roles ──────────────────────────────────────────────────────────────
    public static class Roles
    {
        public const string Base = "api/roles";

        public static string ById(Guid id) => $"api/roles/{id}";
        public static string Assign(Guid id) => $"api/roles/{id}/assign";
        public static string Revoke(Guid id) => $"api/roles/{id}/revoke";
    }

    // ── Tenants ────────────────────────────────────────────────────────────
    public static class Tenants
    {
        public const string Base = "api/tenants";

        public static string ById(Guid id) => $"api/tenants/{id}";
        public static string Settings(Guid id) => $"api/tenants/{id}/settings";
        public static string Languages(Guid id) => $"api/tenants/{id}/languages";
        public static string Delete(Guid id) => $"api/tenants/{id}";

        // ── Provisioning ───────────────────────────────────────────────────
        public const string Provision = "api/tenants/provision";
        public static string ProvisioningStatus(Guid id) => $"api/tenants/{id}/provisioning-status";

        // ── Status & Feature Flags ─────────────────────────────────────────
        public static string Status(Guid id) => $"api/tenants/{id}/status";
        public static string FeatureFlags(Guid id) => $"api/tenants/{id}/feature-flags";
    }

    // ── Approvals ──────────────────────────────────────────────────────────
    public static class Approvals
    {
        public const string Base = "api/approvals";

        public static string ById(Guid id) => $"api/approvals/{id}";
        public static string Approve(Guid id) => $"api/approvals/{id}/approve";
        public static string Reject(Guid id) => $"api/approvals/{id}/reject";
    }

    // ── Audit Logs ─────────────────────────────────────────────────────────
    public static class AuditLogs
    {
        public const string Base = "api/audit-logs";
    }

    // ── Dashboard ──────────────────────────────────────────────────────────
    public static class Dashboard
    {
        public const string Stats = "api/dashboard/stats";
        public const string Calendar = "api/schedule/calendar";
    }

    // ── SEO ────────────────────────────────────────────────────────────────
    public static class Seo
    {
        public static string ContentMeta(string slug) => $"api/seo/contents/{slug}/meta";
    }

    // ── SuperAdmin ─────────────────────────────────────────────────────────
    public static class SuperAdmin
    {
        public const string Admins = "api/superadmin/admins";
        public const string Grant = "api/superadmin/grant";
        public const string Revoke = "api/superadmin/revoke";
        public const string Audit = "api/superadmin/audit";

        // Tenants
        public static string TenantDetail(Guid tenantId) => $"api/superadmin/tenants/{tenantId}";
        public static string TenantActive(Guid tenantId) => $"api/superadmin/tenants/{tenantId}/active";
        public static string TenantMaintenance(Guid tenantId) => $"api/superadmin/tenants/{tenantId}/maintenance";
        public static string AssignUser(Guid tenantId) => $"api/superadmin/tenants/{tenantId}/assign-user";
        public static string RemoveUser(Guid tenantId, Guid userId) => $"api/superadmin/tenants/{tenantId}/users/{userId}";
        public static string TenantStatus(Guid tenantId) => $"api/superadmin/tenants/{tenantId}/status";
        public static string TenantsWithDetails() => "api/superadmin/tenants?includeDetails=true";

        // Users
        public static string TransferPreview(Guid userId) => $"api/superadmin/users/{userId}/transfer-preview";
        public const string Transfer = "api/superadmin/users/transfer";
    }
}
