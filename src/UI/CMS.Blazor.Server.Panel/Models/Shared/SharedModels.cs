namespace CMS.Blazor.Server.Panel.Models.Shared;

// ── HTTP sonuç sarmalayıcı ─────────────────────────────────────────────────
// API hataları ProblemDetails formatını izler: { title, detail, status, type }
public sealed record ApiError(string? Title, string? Detail, int? Status, string? Type)
{
    public string Code => Title ?? "Error";
    public string Message => Detail ?? Title ?? "Bilinmeyen hata";
}

public sealed class ApiResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResult<T> Ok(T data) => new() { IsSuccess = true, Data = data };
    public static ApiResult<T> Fail(string code, string msg)
        => new() { IsSuccess = false, Error = new ApiError(code, msg, null, null) };
    public static ApiResult<T> Fail(Exception ex) => Fail("NetworkError", ex.Message);
}

// ── Paylaşılan lock modeli (Content ve Page her ikisinde de kullanılır) ────
public sealed record ContentLockDto(
    Guid EntityId,
    string EntityType,
    bool IsLocked,
    string? LockType,
    Guid? LockedByUserId,
    string? LockedByEmail,
    DateTime? LockedAt,
    string? LockReason = null,
    bool RequiresSuperAdmin = false);

public sealed record SetContentLockRequest(bool IsLocked, string? LockType = "Soft", string? LockReason = null, bool RequiresSuperAdmin = false);

// ── Permission ────────────────────────────────────────────────────────────
public sealed record PermissionTreeDto(int Version, List<PermissionNodeDto> Tree);
public sealed record PermissionNodeDto(
    string Key, string DisplayName, bool Granted, List<PermissionNodeDto> Children);