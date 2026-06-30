using Microsoft.EntityFrameworkCore;
using SVDSystem.Domain.Entities.Analysis;

namespace SVDSystem.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for SVDSystem.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<RepositoryConfiguration> RepositoryConfigurations => Set<RepositoryConfiguration>();
    public DbSet<PromptTemplate> PromptTemplates => Set<PromptTemplate>();
    public DbSet<FilterGroup> FilterGroups => Set<FilterGroup>();
    public DbSet<FileTypeFilter> FileTypeFilters => Set<FileTypeFilter>();
    public DbSet<CategoryGroup> CategoryGroups => Set<CategoryGroup>();
    public DbSet<VulnerabilityCategory> VulnerabilityCategories => Set<VulnerabilityCategory>();
    public DbSet<UserRepositoryAccess> UserRepositoryAccesses => Set<UserRepositoryAccess>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.HasIndex(e => e.ObjectId).IsUnique();
            entity.Property(e => e.ObjectId).HasColumnName("object_id").HasMaxLength(100).IsRequired();
            entity.Property(e => e.DisplayName).HasColumnName("display_name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(200).IsRequired();
        });

        // ── RepositoryConfiguration ──────────────────────────────────────────
        modelBuilder.Entity<RepositoryConfiguration>(entity =>
        {
            entity.ToTable("repository_configurations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.HasIndex(e => e.RepositoryId).IsUnique();
            entity.Property(e => e.RepositoryId).HasColumnName("repository_id").HasMaxLength(100).IsRequired();
            entity.Property(e => e.RepositoryName).HasColumnName("repository_name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.ProjectName).HasColumnName("project_name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.RemoteUrl).HasColumnName("remote_url").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Enabled).HasColumnName("enabled").HasDefaultValue(true);
            entity.Property(e => e.CustomPrompt).HasColumnName("custom_prompt");
            entity.Property(e => e.SeverityThreshold).HasColumnName("severity_threshold").HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.VulnerabilityCategories).HasColumnName("vulnerability_categories").HasDefaultValue(string.Empty);
            entity.Property(e => e.IgnorePaths).HasColumnName("ignore_paths").HasDefaultValue(string.Empty);
            entity.Property(e => e.FileTypeFilters).HasColumnName("file_type_filters").HasDefaultValue(string.Empty);
            entity.Property(e => e.IncludeAddedFiles).HasColumnName("include_added_files").HasDefaultValue(true);
            entity.Property(e => e.IncludeDeletedFiles).HasColumnName("include_deleted_files").HasDefaultValue(true);
            entity.Property(e => e.IncludeModifiedFiles).HasColumnName("include_modified_files").HasDefaultValue(true);
            entity.Property(e => e.UseCategories).HasColumnName("use_categories").HasDefaultValue(false);
        });

        // ── PromptTemplate ───────────────────────────────────────────────────
        modelBuilder.Entity<PromptTemplate>(entity =>
        {
            entity.ToTable("prompt_templates");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Content).HasColumnName("content").IsRequired();
            entity.Property(e => e.IsSystem).HasColumnName("is_system").HasDefaultValue(false);
            entity.HasOne(e => e.CreatedBy).WithMany()
                  .HasForeignKey("created_by_id")
                  .HasConstraintName("fk_prompt_templates_created_by")
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property<Guid>("created_by_id").HasColumnName("created_by_id").IsRequired();
        });

        // ── FilterGroup ──────────────────────────────────────────────────────
        modelBuilder.Entity<FilterGroup>(entity =>
        {
            entity.ToTable("filter_groups");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.IgnorePaths).HasColumnName("ignore_paths").HasDefaultValue(string.Empty);
            entity.Property(e => e.FileTypeExtensions).HasColumnName("file_type_extensions").HasDefaultValue(string.Empty);
            entity.HasOne(e => e.CreatedBy).WithMany()
                  .HasForeignKey("created_by_id")
                  .HasConstraintName("fk_filter_groups_created_by")
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property<Guid>("created_by_id").HasColumnName("created_by_id").IsRequired();
        });

        // ── FileTypeFilter ───────────────────────────────────────────────────
        modelBuilder.Entity<FileTypeFilter>(entity =>
        {
            entity.ToTable("file_type_filters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Extension).HasColumnName("extension").HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.CreatedBy).WithMany()
                  .HasForeignKey("created_by_id")
                  .HasConstraintName("fk_file_type_filters_created_by")
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property<Guid>("created_by_id").HasColumnName("created_by_id").IsRequired();
        });

        // ── CategoryGroup ────────────────────────────────────────────────────
        modelBuilder.Entity<CategoryGroup>(entity =>
        {
            entity.ToTable("category_groups");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Categories).HasColumnName("categories").HasDefaultValue(string.Empty);
            entity.HasOne(e => e.CreatedBy).WithMany()
                  .HasForeignKey("created_by_id")
                  .HasConstraintName("fk_category_groups_created_by")
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property<Guid>("created_by_id").HasColumnName("created_by_id").IsRequired();
        });

        // ── VulnerabilityCategory ────────────────────────────────────────────
        modelBuilder.Entity<VulnerabilityCategory>(entity =>
        {
            entity.ToTable("vulnerability_categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.HasOne(e => e.CreatedBy).WithMany()
                  .HasForeignKey("created_by_id")
                  .HasConstraintName("fk_vulnerability_categories_created_by")
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property<Guid>("created_by_id").HasColumnName("created_by_id").IsRequired();
        });

        // ── UserRepositoryAccess ─────────────────────────────────────────────
        modelBuilder.Entity<UserRepositoryAccess>(entity =>
        {
            entity.ToTable("user_repository_accesses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property<Guid>("user_id").HasColumnName("user_id").IsRequired();
            entity.Property<Guid>("repository_configuration_id").HasColumnName("repository_configuration_id").IsRequired();
            entity.HasIndex("user_id", "repository_configuration_id").IsUnique();
            entity.HasOne(e => e.User).WithMany()
                  .HasForeignKey("user_id")
                  .HasConstraintName("fk_user_repository_accesses_user")
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.RepositoryConfiguration).WithMany(e => e.UserAccess)
                  .HasForeignKey("repository_configuration_id")
                  .HasConstraintName("fk_user_repository_accesses_repo")
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
