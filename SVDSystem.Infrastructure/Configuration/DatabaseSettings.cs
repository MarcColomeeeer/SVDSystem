namespace SVDSystem.Infrastructure.Configuration;

public class DatabaseSettings
{
    public const string SectionName = "Database";

    /// <summary>
    /// PostgreSQL connection string.
    /// Example: "Host=localhost;Port=5432;Database=svdsystem;Username=postgres;Password=secret"
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Apply pending EF Core migrations automatically on startup.
    /// Recommended for development; disable in production and run migrations manually.
    /// </summary>
    public bool ApplyMigrationsOnStartup { get; set; } = true;
}
