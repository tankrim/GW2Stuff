using BarFoo.Domain.Entities;

using Microsoft.EntityFrameworkCore;
namespace BarFoo.Data.Contexts;

/// <summary>
/// Database context for managing Guild Wars 2 API keys and objectives.
/// Handles relationships between accounts and their associated game objectives.
/// There are three related database tables:
/// ApiKeys: Stores API keys associated with account names, tracking sync status and timing.
/// Objectives: Stores game objectives with properties like title, progress, and claim status.
/// ApiKeyObjectives: A junction table that creates a many-to-many relationship between API keys and objectives.
/// </summary>
public class BarFooDbContext : DbContext
{
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<Objective> Objectives { get; set; }
    public DbSet<ApiKeyObjective> ApiKeyObjectives { get; set; }

    public BarFooDbContext(DbContextOptions<BarFooDbContext> options)
        : base(options)
    { }

    public BarFooDbContext()
        : base()
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Name);

            entity.Property(e => e.Name)
                .IsRequired();

            entity.Property(e => e.Key)
                .IsRequired();

            entity.Property(e => e.HasBeenSyncedOnce)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.LastSyncTime)
                .IsRequired()
                .HasDefaultValueSql("datetime('now')");
        });

        modelBuilder.Entity<Objective>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired();

            entity.Property(e => e.Track)
                .IsRequired();

            entity.Property(e => e.Acclaim)
                .IsRequired();

            entity.Property(e => e.ProgressCurrent)
                .IsRequired();

            entity.Property(e => e.ProgressComplete)
                .IsRequired();

            entity.Property(e => e.Claimed)
                .IsRequired();

            entity.Property(e => e.ApiEndpoint)
                .IsRequired();
        });

        modelBuilder.Entity<ApiKeyObjective>(entity =>
        {
            entity.HasKey(e => new { e.ApiKeyName, e.ObjectiveId });

            entity.HasOne(ao => ao.ApiKey)
                .WithMany(a => a.ApiKeyObjectives)
                .HasForeignKey(ao => ao.ApiKeyName)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ao => ao.Objective)
                .WithMany(o => o.ApiKeyObjectives)
                .HasForeignKey(ao => ao.ObjectiveId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
