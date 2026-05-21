using CMS.Blazor.Server.Panel.Models.Shared;
using CMS.Blazor.Server.Panel.Services.State;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CMS.Blazor.Server.Panel.Infrastructure.Http.Base;

/// <summary>
/// Tüm API istemcileri bu sınıftan türer.
/// HTTP altyapısı (Build, ParseAsync, BuildQuery) tek yerde yaşar — SRP gereği.
/// </summary>
public abstract class ApiClientBase(IHttpClientFactory factory, ApplicationState appState)
{
    public const string ClientName = "CmsApi";

    protected static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    // ── Protected HTTP helpers ────────────────────────────────────────────

    protected async Task<ApiResult<TRes>> GetAsync<TRes>(
        string url, CancellationToken ct, string? tokenOverride = null)
    {
        using var client = Build(tokenOverride: tokenOverride);
        try { return await ParseAsync<TRes>(await client.GetAsync(url, ct)); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { return ApiResult<TRes>.Fail(ex); }
    }

    protected async Task<ApiResult<TRes>> PostAsync<TReq, TRes>(
        string url, TReq body, bool auth = true, CancellationToken ct = default)
    {
        using var client = Build(includeAuth: auth);
        try { return await ParseAsync<TRes>(await client.PostAsJsonAsync(url, body, JsonOpts, ct)); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { return ApiResult<TRes>.Fail(ex); }
    }

    protected async Task<ApiResult<TRes>> PutAsync<TReq, TRes>(
        string url, TReq body, CancellationToken ct)
    {
        using var client = Build();
        try { return await ParseAsync<TRes>(await client.PutAsJsonAsync(url, body, JsonOpts, ct)); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { return ApiResult<TRes>.Fail(ex); }
    }

    protected async Task<ApiResult<T>> DeleteAsync<T>(string url, CancellationToken ct)
    {
        using var client = Build();
        try { return await ParseAsync<T>(await client.DeleteAsync(url, ct)); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { return ApiResult<T>.Fail(ex); }
    }

    /// <summary>DELETE with JSON body — HTTP spec'e aykırı ama bazı endpointler ister (ör: /roles/{id}/revoke).</summary>
    protected async Task<ApiResult<TRes>> DeleteWithBodyAsync<TReq, TRes>(
        string url, TReq body, CancellationToken ct)
    {
        using var client = Build();
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url)
            {
                Content = JsonContent.Create(body, options: JsonOpts)
            };
            return await ParseAsync<TRes>(await client.SendAsync(request, ct));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { return ApiResult<TRes>.Fail(ex); }
    }

    // ── Query string builder ──────────────────────────────────────────────

    protected static string BuildQuery(params (string Key, object? Value)[] pairs)
    {
        var parts = pairs
            .Where(p => p.Value is not null && p.Value.ToString() != string.Empty)
            .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!.ToString()!)}");
        var q = string.Join("&", parts);
        return string.IsNullOrEmpty(q) ? string.Empty : "?" + q;
    }

    // ── Private HTTP client factory ───────────────────────────────────────

    private HttpClient Build(bool includeAuth = true, string? tokenOverride = null)
    {
        var client = factory.CreateClient(ClientName);
        var session = appState.Session;

        if (includeAuth)
        {
            var token = tokenOverride ?? session.Token;
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        client.DefaultRequestHeaders.Remove("Accept-Language");
        client.DefaultRequestHeaders.Add("Accept-Language", appState.Preferences.Language);

        if (session.CurrentTenantId.HasValue)
        {
            client.DefaultRequestHeaders.Remove("X-Tenant-Id");
            client.DefaultRequestHeaders.Add("X-Tenant-Id",
                session.CurrentTenantId.Value.ToString());
        }

        return client;
    }

    // ── Response parser ───────────────────────────────────────────────────

    private static async Task<ApiResult<T>> ParseAsync<T>(HttpResponseMessage r)
    {
        var body = await r.Content.ReadAsStringAsync();

        if (r.IsSuccessStatusCode)
        {
            if (r.StatusCode == System.Net.HttpStatusCode.NoContent)
                return ApiResult<T>.Ok(default!);
            if (string.IsNullOrWhiteSpace(body))
                return ApiResult<T>.Ok(default!);
            try
            {
                var data = JsonSerializer.Deserialize<T>(body, JsonOpts);
                return ApiResult<T>.Ok(data!);
            }
            catch (JsonException ex)
            {
                return ApiResult<T>.Fail("ParseError",
                    $"JSON parse hatası: {ex.Message}\nBody: {body[..Math.Min(200, body.Length)]}");
            }
        }

        // ProblemDetails: { title, detail, status, type }
        try
        {
            var err = JsonSerializer.Deserialize<ApiError>(body, JsonOpts);
            return ApiResult<T>.Fail(err?.Code ?? "Error", err?.Message ?? body);
        }
        catch
        {
            return ApiResult<T>.Fail($"HTTP{(int)r.StatusCode}", body);
        }
    }
}
