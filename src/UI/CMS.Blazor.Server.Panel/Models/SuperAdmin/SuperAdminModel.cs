namespace CMS.Blazor.Server.Panel.Models.SuperAdmin;

/// <summary>JWT claim'inden elde edilir; session'a yazılır.</summary>
public sealed record SuperAdminSessionDto(
    Guid UserId, string Email, bool IsSuperAdmin, DateTime? SuperAdminGrantedAt);

public sealed record SuperAdminListDto(
    Guid UserId, string Email, string? DisplayName, DateTime GrantedAt,
    string? GrantedByEmail = null, bool IsActive = true);

public sealed record GrantSuperAdminRequest(Guid UserId, string? ConfirmPhrase = null);
public sealed record RevokeSuperAdminRequest(Guid UserId, string? Reason = null);

public sealed record UserTransferPreviewDto(
    Guid UserId, string Email, string? DisplayName,
    Guid SourceTenantId, string SourceTenantName,
    List<string> CurrentRoles,
    bool HasActiveSession = false,
    int OwnedContentCount = 0);

public sealed record TransferUserRequest(
    Guid UserId, Guid SourceTenantId, Guid TargetTenantId,
    List<Guid>? RoleIds = null, bool KeepSource = false, bool TransferContent = false);

public sealed record TransferUserResponse(
    bool Success, string? Message, int ContentTransferred, List<string> Warnings);

public sealed record AssignUserToTenantRequest(Guid UserId, Guid TenantId, List<Guid> RoleIds);

/// <summary>SuperAdmin rol sabitlerini ve yardımcı metodları barındırır.</summary>
public static class SuperAdminClaims
{
    public const string RoleName = "superadmin";
    public const string ClaimType = "role";
    public const string LogDisplayName = "superadmin";

    public static bool IsSuperAdminRole(string roleName) =>
        string.Equals(roleName, RoleName, StringComparison.OrdinalIgnoreCase);

    public static bool HasSuperAdmin(IEnumerable<string> roles) =>
        roles.Any(IsSuperAdminRole);
}