using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using SVDSystem.Application.Interfaces;
using SVDSystem.Infrastructure.Configuration;
using SVDSystem.Infrastructure.Persistence;
using SVDSystem.Infrastructure.Repositories;
var builder = WebApplication.CreateBuilder(args);

// ── Authentication — Azure Entra ID ───────────────────────────────────────────
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

// ── CORS — allow the React dev server and configured origins ──────────────────
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

// ── Database ──────────────────────────────────────────────────────────────────
var dbSettings = builder.Configuration
    .GetSection(DatabaseSettings.SectionName)
    .Get<DatabaseSettings>() ?? new DatabaseSettings();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dbSettings.ConnectionString));

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IRepositoryConfigurationRepository, RepositoryConfigurationRepository>();
builder.Services.AddScoped<IPromptTemplateRepository, PromptTemplateRepository>();
builder.Services.AddScoped<IFilterGroupRepository, FilterGroupRepository>();
builder.Services.AddScoped<IFileTypeFilterRepository, FileTypeFilterRepository>();
builder.Services.AddScoped<ICategoryGroupRepository, CategoryGroupRepository>();
builder.Services.AddScoped<IVulnerabilityCategoryRepository, VulnerabilityCategoryRepository>();
builder.Services.AddScoped<IUserRepositoryAccessRepository, UserRepositoryAccessRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ── ASP.NET Core ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ── Migrations + seeding on startup ──────────────────────────────────────────
if (dbSettings.ApplyMigrationsOnStartup)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

