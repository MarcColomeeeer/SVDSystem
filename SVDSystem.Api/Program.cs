using Microsoft.EntityFrameworkCore;
using SVDSystem.Application.Interfaces;
using SVDSystem.Application.Services;
using SVDSystem.Infrastructure.Configuration;
using SVDSystem.Infrastructure.Persistence;
using SVDSystem.Infrastructure.Repositories;
using SVDSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ──────────────────────────────────────────────────────────

builder.Services.Configure<GitSettings>(
    builder.Configuration.GetSection(GitSettings.SectionName));

builder.Services.Configure<OllamaSettings>(
    builder.Configuration.GetSection(OllamaSettings.SectionName));

var dbSettings = builder.Configuration
    .GetSection(DatabaseSettings.SectionName)
    .Get<DatabaseSettings>() ?? new DatabaseSettings();

// ── Database ───────────────────────────────────────────────────────────────

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dbSettings.ConnectionString));

// ── Application services ───────────────────────────────────────────────────

builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddScoped<IRepositoryConfigurationRepository, RepositoryConfigurationRepository>();
builder.Services.AddSingleton<IDiffParserService, DiffParserService>();

// ── Infrastructure services ────────────────────────────────────────────────

builder.Services.AddSingleton<IGitService, GitService>();

builder.Services.AddHttpClient<IOllamaService, OllamaService>((sp, client) =>
{
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
});

builder.Services.AddHttpClient<IAzureDevOpsService, AzureDevOpsService>();

// ── ASP.NET Core ───────────────────────────────────────────────────────────

builder.Services.AddControllers();

var app = builder.Build();

// ── Ensure DB schema exists on startup ────────────────────────────────────────

if (dbSettings.ApplyMigrationsOnStartup)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();


