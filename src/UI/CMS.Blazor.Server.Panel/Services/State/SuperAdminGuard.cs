using CMS.Blazor.Server.Panel.Models.App;
using CMS.Blazor.Server.Panel.Models.Role;
using CMS.Blazor.Server.Panel.Models.Shared;
using CMS.Blazor.Server.Panel.Models.SuperAdmin;
using CMS.Blazor.Server.Panel.Models.User;

namespace CMS.Blazor.Server.Panel.Services.State;

/// <summary>
/// Uygulama içinde superadmin kontrollerini merkezi yönetir.
/// Scoped olarak kayıt edilmeli (her kullanıcı oturumu için ayrı).
/// </summary>
public sealed class SuperAdminGuard(ApplicationState appState)
{
    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Mevcut kullanıcı superadmin mi?</summary>
    public bool IsSuperAdmin =>
        appState.Session.Roles.Any(SuperAdminClaims.IsSuperAdminRole);

    /// <summary>
    /// Mevcut kullanıcı admin mi? (superadmin dahil).
    /// Admin UI kilitlemek için kullanılır.
    /// </summary>
    public bool IsAdminOrAbove =>
        IsSuperAdmin ||
        appState.Session.Can("users.manage") ||
        appState.Session.Can("tenants.manage");

    /// <summary>
    /// Belirtilen rol adı gizli superadmin rolü mü?
    /// Diğer kullanıcıların rol listelerinde bu rol GÖSTERILMEZ.
    /// </summary>
    public static bool IsHiddenRole(string roleName, bool viewerIsSuperAdmin) =>
        SuperAdminClaims.IsSuperAdminRole(roleName) && !viewerIsSuperAdmin;

    /// <summary>
    /// Verilen rol listesinden superadmin rollerini filtrele.
    /// viewerIsSuperAdmin = true ise filtreleme yapılmaz.
    /// </summary>
    public List<string> FilterRoles(IEnumerable<string> roles) =>
        IsSuperAdmin
            ? roles.ToList()
            : roles.Where(r => !SuperAdminClaims.IsSuperAdminRole(r)).ToList();

    /// <summary>
    /// Verilen kullanıcı listesinden superadminleri filtrele.
    /// SuperAdmin kullanıcıları başkalarının kullanıcı listesinde görünmez.
    /// </summary>
    public List<UserListDto> FilterUsers(IEnumerable<UserListDto> users) =>
        IsSuperAdmin
            ? users.ToList()
            : users.Where(u => !u.Roles.Any(SuperAdminClaims.IsSuperAdminRole)).ToList();

    /// <summary>Verilen rol listesi görüntülenebilir mi?</summary>
    public List<RoleListDto> FilterRoleList(IEnumerable<RoleListDto> roles) =>
        IsSuperAdmin
            ? roles.ToList()
            : roles.Where(r => !SuperAdminClaims.IsSuperAdminRole(r.Name)).ToList();

    // ── Kilit kontrolleri ─────────────────────────────────────────────────────

    /// <summary>İçerik kilitli mi ve mevcut kullanıcı kilitli içeriği düzenleyebilir mi?</summary>
    public bool CanEditLocked(ContentLockDto? lockInfo)
    {
        if (lockInfo is null || !lockInfo.IsLocked) return true;
        //if (lockInfo.RequiresSuperAdmin) return IsSuperAdmin;
        return IsAdminOrAbove;
    }

    /// <summary>İçeriği kilitleme yetkisi var mı?</summary>
    public bool CanLock => IsAdminOrAbove;

    /// <summary>Superadmin gerekli kilidi kaldırabilir mi?</summary>
    public bool CanUnlockSuperAdminLock => IsSuperAdmin;

    // ── Tenant kontrolleri ────────────────────────────────────────────────────

    /// <summary>Tenant yönetimi (aktif/pasif, bakım modu) için superadmin gerekli</summary>
    public bool CanManageTenantStatus => IsSuperAdmin;

    /// <summary>Kullanıcı transferi için superadmin gerekli</summary>
    public bool CanTransferUsers => IsSuperAdmin;
}

/// <summary>
/// ApplicationState.Session'a SuperAdmin bilgisi ekler.
/// Mevcut SessionState sınıfına extension olarak çalışır.
/// </summary>
public static class SessionStateExtensions
{
    /// <summary>Oturumun superadmin rolü içerip içermediğini döner</summary>
    public static bool IsSuperAdmin(this UserSession session) =>
        session.Roles.Any(SuperAdminClaims.IsSuperAdminRole);

    /// <summary>
    /// SuperAdmin veya belirli bir izne sahip mi?
    /// SuperAdmin her zaman true döner (her yetkiye sahip).
    /// </summary>
    public static bool CanOrSuperAdmin(this UserSession session, string permission) =>
        session.IsSuperAdmin() || session.Can(permission);
}
