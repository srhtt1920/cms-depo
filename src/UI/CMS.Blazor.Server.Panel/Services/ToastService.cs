namespace CMS.Blazor.Server.Panel.Services;

/// <summary>
/// Uygulama genelinde toast bildirimleri göstermek için servis.
/// DashBoardLayout içindeki <ToastContainer /> bileşeni bu servisi dinler.
/// </summary>
public sealed class ToastService
{
    private readonly List<ToastMessage> _toasts = [];
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>Yeni bir toast eklendiğinde tetiklenir (ToastContainer bunu dinler).</summary>
    public event Action? OnChanged;

    public IReadOnlyList<ToastMessage> Toasts => _toasts;

    // ── Public API ──────────────────────────────────────────────────────────

    public void Success(string message, string? title = null, int durationMs = 3500)
        => Add(ToastLevel.Success, title ?? "Başarılı", message, durationMs);

    public void Error(string message, string? title = null, int durationMs = 6000)
        => Add(ToastLevel.Error, title ?? "Hata", message, durationMs);

    public void Warning(string message, string? title = null, int durationMs = 4500)
        => Add(ToastLevel.Warning, title ?? "Uyarı", message, durationMs);

    public void Info(string message, string? title = null, int durationMs = 4000)
        => Add(ToastLevel.Info, title ?? "Bilgi", message, durationMs);

    /// <summary>
    /// Bir API çağrısından dönen sonuca göre otomatik toast gösterir.
    /// Başarılıysa Success, değilse Error mesajı basılır.
    /// </summary>
    public void FromResult(bool isSuccess, string successMsg, string? errorMsg = null,
        string? successTitle = null, string? errorTitle = null)
    {
        if (isSuccess) Success(successMsg, successTitle);
        else Error(errorMsg ?? "Bir hata oluştu.", errorTitle);
    }

    public void Dismiss(Guid id)
    {
        _lock.Wait();
        try
        {
            var t = _toasts.FirstOrDefault(t => t.Id == id);
            if (t is not null) _toasts.Remove(t);
        }
        finally { _lock.Release(); }
        OnChanged?.Invoke();
    }

    // ── Internal ────────────────────────────────────────────────────────────

    private void Add(ToastLevel level, string title, string message, int durationMs)
    {
        var toast = new ToastMessage(Guid.NewGuid(), level, title, message, durationMs);
        _lock.Wait();
        try { _toasts.Insert(0, toast); }
        finally { _lock.Release(); }
        OnChanged?.Invoke();
    }
}

// ── Models ──────────────────────────────────────────────────────────────────

public sealed class ToastMessage(
    Guid id, ToastLevel level, string title, string message, int durationMs)
{
    public Guid Id { get; } = id;
    public ToastLevel Level { get; } = level;
    public string Title { get; } = title;
    public string Message { get; } = message;
    public int DurationMs { get; } = durationMs;
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public enum ToastLevel { Success, Error, Warning, Info }
