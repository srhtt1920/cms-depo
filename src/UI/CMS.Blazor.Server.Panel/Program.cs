using CMS.Blazor.Server.Panel;
using CMS.Blazor.Server.Panel.Infrastructure.Auth;
using CMS.Blazor.Server.Panel.Infrastructure.Http.Base;
using CMS.Blazor.Server.Panel.Infrastructure.Preferences;
using CMS.Blazor.Server.Panel.Services;
using CMS.Blazor.Server.Panel.Services.State;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();

// ── Circuit state (Memory veya Redis) ─────────────────────────────────────
// appsettings: Session:CircuitStorage = "Memory" | "Redis"
var circuitStorage = builder.Configuration["Session:CircuitStorage"] ?? "Memory";

if (circuitStorage.Equals("Redis", StringComparison.OrdinalIgnoreCase))
{
    var redisConn = builder.Configuration.GetConnectionString("Redis")
        ?? throw new InvalidOperationException("Redis bağlantı dizesi eksik.");

    // AddHybridCache ile Memory L1 + Redis L2 hibrit önbellek.
    // .AddStackExchangeRedisL2Cache NuGet paketi:
    //   Microsoft.Extensions.Caching.StackExchangeRedis
    // Derleme hatasına neden oluyorsa paketi yükleyin:
    //   dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
    builder.Services.AddHybridCache();
    // .AddStackExchangeRedisL2Cache(redisConn);   ← paketi ekledikten sonra aktif edin
}

// ── API HttpClient ─────────────────────────────────────────────────────────
var apiBase = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Api:BaseUrl appsettings.json'da eksik.");

builder.Services.AddHttpClient(ApiClientBase.ClientName, client =>
{
    client.BaseAddress = new Uri(apiBase);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ── Auth ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<CmsAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<CmsAuthStateProvider>());
builder.Services.AddAuthorizationCore();

// ── Servisler ─────────────────────────────────────────────────────────────
builder.Services.AddCmsApiClients();
builder.Services.AddScoped<CookiePreferencesService>(); // JS cookie okuma/yazma
builder.Services.AddScoped<ApplicationState>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PermissionSyncService>();
builder.Services.AddScoped<TenantLanguageState>();
builder.Services.AddScoped<NavMenuState>();
builder.Services.AddScoped<SuperAdminGuard>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddSingleton<SvgIconService>();

// ── Pipeline ──────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();