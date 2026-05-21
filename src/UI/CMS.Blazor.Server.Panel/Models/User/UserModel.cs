namespace CMS.Blazor.Server.Panel.Models.User;

public sealed record UserListDto(
    Guid Id, string Email, string? DisplayName,
    List<string> Roles, bool IsActive,
    DateTime? LastLogin,
    int PermissionVersion);

public sealed record CreateUserApiRequest(string Email, string Password, List<Guid> RoleIds);
public sealed record CreateUserResponse(Guid Id, string Email, List<string> Roles, bool IsActive);
public sealed record UpdateRolesApiRequest(List<Guid> RoleIds);
public sealed record SetActiveApiRequest(bool IsActive);

public sealed record UserActivityPageDto(int TotalCount, List<UserActivityItemDto> Items);
public sealed record UserActivityItemDto(
    Guid Id, string Action, string EntityType,
    string? EntityId, string? Details, string? IpAddress,
    string Icon, string Color, DateTime OccurredAt);

// ── Invite System ──────────────────────────────────────────────────────────
public enum InviteStatus { Pending, Accepted, Rejected, Expired, Cancelled }

public sealed record InviteUserRequest(
    string Email,
    string? FirstName,
    string? LastName,
    List<Guid> RoleIds,
    string? PersonalMessage = null,
    int ExpirationDays = 7);

public sealed record InviteDto(
    Guid Id,
    string Token,
    string Email,
    string? DisplayName,
    Guid TenantId,
    string TenantName,
    List<string> Roles,
    InviteStatus Status,
    DateTime ExpiresAt,
    DateTime SentAt,
    DateTime? RespondedAt,
    Guid InvitedByUserId,
    string InvitedByEmail);

public sealed record AcceptInviteRequest(
    string Token,
    string Password,
    string ConfirmPassword,
    string? DisplayName = null);

public sealed record ResendInviteRequest(Guid InviteId);

// ── User-Tenant Membership ─────────────────────────────────────────────────
public enum MembershipStatus { PendingInvite, Active, Suspended, Removed }

public sealed record UserTenantMembershipDto(
    Guid UserId,
    Guid TenantId,
    string TenantName,
    List<string> Roles,
    MembershipStatus Status,
    DateTime JoinedAt,
    DateTime? SuspendedAt,
    string? SuspendReason,
    bool IsDefaultTenant);

public sealed record UpdateMembershipStatusRequest(
    MembershipStatus Status,
    string? Reason = null);
