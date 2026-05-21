using CMS.Infrastructure.Persistence.Seed;
using CMS.Infrastructure.SignalR.Hubs;
using CMS.WebApi.Extensions;
using CMS.WebApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// DI kayıtları
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// Response sıkıştırma (gzip + brotli)
builder.Services.AddResponseCompression(opts =>
{
    opts.EnableForHttps = true;
    opts.Providers.Add<BrotliCompressionProvider>();
    opts.Providers.Add<GzipCompressionProvider>();
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/json", "application/xml", "text/plain"]);
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

builder.Services
    .AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
        opts.JsonSerializerOptions.PropertyNamingPolicy =
            JsonNamingPolicy.CamelCase;
    });

// JWT Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };

        // SignalR WebSocket JWT
        opts.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(token) && path.StartsWithSegments("/hubs"))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("SuperAdminOnly", policy =>
       policy.RequireClaim("isSuperAdmin", "true"));
});

// CORS — tenant origin listesi veya appsettings'den okunur
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:5001", "https://localhost:7001"];

builder.Services.AddCors(opts =>
    opts.AddPolicy("CmsPolicy", policy =>
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));

// Rate Limiting — login endpoint brute-force koruması
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", cfg =>
    {
        cfg.PermitLimit = 5;
        cfg.Window = TimeSpan.FromMinutes(1);
        cfg.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        cfg.QueueLimit = 0;
    });
    // Genel API limiti — IP bazlı
    options.AddFixedWindowLimiter("api", cfg =>
    {
        cfg.PermitLimit = 300;
        cfg.Window = TimeSpan.FromMinutes(1);
        cfg.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        cfg.QueueLimit = 10;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddSignalR();

// Health checks
builder.Services
    .AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("SqlServer")!,
        name: "sqlserver",
        failureStatus: HealthStatus.Degraded,
        tags: ["db"])
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

builder.Services
    .AddHealthChecksUI(setup =>
    {
        setup.AddHealthCheckEndpoint("CMS API", "/health");
    })
    .AddInMemoryStorage();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed — sadece Development ortamında veya CMS_RUN_SEED=true ise
var runSeed = app.Environment.IsDevelopment()
    || string.Equals(Environment.GetEnvironmentVariable("CMS_RUN_SEED"), "true",
    StringComparison.OrdinalIgnoreCase);

if (runSeed)
    await DataSeeder.SeedAsync(app.Services, app.Environment.IsDevelopment());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseResponseCompression();
app.UseRateLimiter();
// Security headers
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    ctx.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    if (!app.Environment.IsDevelopment())
        ctx.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    await next();
});

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
});
app.UseCors("CmsPolicy");
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<SerializationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PermissionHub>("/hubs/permissions");

// Health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds
            })
        };
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(result));
    }
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{ Predicate = r => r.Tags.Contains("live") });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{ Predicate = r => r.Tags.Contains("db") });

app.Run();
