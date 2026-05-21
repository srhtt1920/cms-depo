using CMS.Domain.Common;
using CMS.Domain.Identity.Events;
using CMS.Domain.Tenants;

namespace CMS.Domain.Identity;

public sealed class User : AggregateRoot<UserId>
{

    private readonly List<UserTenantRole> _tenantRoles = [];

    public string        Email              { get; private set; } = default!;
    public string        PasswordHash       { get; private set; } = default!;
    public string?       DisplayName        { get; private set; }
    public bool          IsActive           { get; private set; } = true;
    public bool          IsSuperAdmin       { get; private set; }
    public int           PermissionVersion  { get; private set; } = 1;
    public DateTime      CreatedAt          { get; private set; }
    public DateTime?     UpdatedAt          { get; private set; }
    public DateTime?     LastLoginAt        { get; private set; }
    public DateTime?     SuperAdminGrantedAt{ get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }
    public IReadOnlyList<UserTenantRole> TenantRoles => _tenantRoles.AsReadOnly();
    public bool IsTwoFactorEnabled { get; private set; } = false;
    public string? TwoFactorSecret { get; private set; }
    private List<string> _backupCodes = [];
    public IReadOnlyList<string> BackupCodes => _backupCodes.AsReadOnly();
    public string? EmailOtp { get; private set; }
    public DateTime? EmailOtpExpiresAt { get; private set; }


    private User() { } // EF Core

    public static User Create(string email, string passwordHash, string? displayName = null)
    {
        var user = new User
        {
            Id            = UserId.New(),
            Email         = email.ToLowerInvariant().Trim(),
            PasswordHash  = passwordHash,
            DisplayName   = displayName,
            CreatedAt     = DateTime.UtcNow
        };
        user.AddDomainEvent(new UserCreatedEvent(user.Id, user.Email));
        return user;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt   = DateTime.UtcNow;
    }

    public void UpdateProfile(string? displayName, string email)
    {
        DisplayName  = displayName;
        Email        = email.ToLowerInvariant().Trim();
        UpdatedAt    = DateTime.UtcNow;
    }

    public void ChangePassword(string newHash)
    {
        PasswordHash = newHash;
        UpdatedAt    = DateTime.UtcNow;
    }

    public void AssignRole(TenantId tenantId, RoleId roleId)
    {
        if (_tenantRoles.Any(r => r.TenantId == tenantId && r.RoleId == roleId))
            return;

        _tenantRoles.Add(UserTenantRole.Create(Id, tenantId, roleId));
        IncrementPermissionVersion(tenantId);
    }

    public void RevokeRole(TenantId tenantId, RoleId roleId)
    {
        _tenantRoles.RemoveAll(r => r.TenantId == tenantId && r.RoleId == roleId);
        IncrementPermissionVersion(tenantId);
    }

    public void GrantSuperAdmin()
    {
        IsSuperAdmin = true;
        SuperAdminGrantedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RevokeSuperAdmin()
    {
        IsSuperAdmin = false;
        SuperAdminGrantedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()      { IsActive = true;   UpdatedAt = DateTime.UtcNow; }
    
    public void Deactivate()    { IsActive = false;  UpdatedAt = DateTime.UtcNow; }

    public void SetPasswordResetToken(string token, DateTime expiresAt)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsPasswordResetTokenValid(string token) =>
        PasswordResetToken == token &&
        PasswordResetTokenExpiresAt.HasValue &&
        PasswordResetTokenExpiresAt.Value > DateTime.UtcNow;

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    private void IncrementPermissionVersion(TenantId tenantId)
    {
        PermissionVersion++;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new PermissionVersionIncrementedEvent(Id, tenantId, PermissionVersion));
    }

    public IEnumerable<TenantId> GetTenantIds() =>
        _tenantRoles.Select(r => r.TenantId).Distinct();

    public void Setup2Fa(string secret, string[] backupCodes)
    {
        TwoFactorSecret = secret;
        IsTwoFactorEnabled = true;
        _backupCodes = backupCodes.ToList();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable2Fa()
    {
        IsTwoFactorEnabled = false;
        TwoFactorSecret = null;
        _backupCodes.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    public bool UseBackupCode(string code)
    {
        var index = _backupCodes.IndexOf(code);
        if (index < 0) return false;
        _backupCodes.RemoveAt(index); // tek kullaným
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public void SetEmailOtp(string otp, DateTime expiresAt)
    {
        EmailOtp = otp;
        EmailOtpExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }
}