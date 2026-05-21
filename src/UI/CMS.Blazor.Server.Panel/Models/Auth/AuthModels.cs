namespace CMS.Blazor.Server.Panel.Models.Auth;

// ── Login / Token ──────────────────────────────────────────────────────────
public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    string Email,
    List<TenantInfo> Tenants,
    string? RefreshToken = null,
    bool RequiresTwoFactor = false,
    string? DisplayName = null);

public sealed record TenantInfo(Guid TenantId, string Name, bool IsSystem);

public sealed record SelectTenantResponse(
    string Token, DateTime ExpiresAt, Guid TenantId, string Name);

public sealed record RefreshTokenRequest(string Token);

public sealed record LogoutRequest(string? RefreshToken = null);

// ── Profile ────────────────────────────────────────────────────────────────
public sealed record UpdateProfileRequest(string? DisplayName, string Email);

public sealed record UpdateProfileResponse(
    Guid Id, string Email, string? DisplayName, DateTime? LastLoginAt);

public sealed record ChangePasswordRequest(
    string CurrentPassword, string NewPassword, string ConfirmPassword);

// ── Password Reset ─────────────────────────────────────────────────────────
public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(string Token, string Email, string NewPassword);

// ── Two-Factor ─────────────────────────────────────────────────────────────
public sealed record TwoFactorSetupResponse(
    string QrCodeUri, string ManualKey, string[] BackupCodes);

public sealed record TwoFactorVerifyRequest(string Code, string Email);

public sealed record TwoFactorVerifyResponse(
    string AccessToken, 
    string RefreshToken, 
    DateTime ExpiresAt,
    string Email,
    string? DisplayName,
    List<TenantInfo> Tenants);

public sealed record TwoFactorDisableRequest(string CurrentPassword);

public sealed record TwoFactorResendRequest(string Email);
