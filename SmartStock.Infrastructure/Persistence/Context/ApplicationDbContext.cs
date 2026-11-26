using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartStock.Domain.Common;
using SmartStock.Domain.Entities;
using SmartStock.Infrastructure.Identity;

namespace SmartStock.Infrastructure.Persistence.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Product Configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");

            // Primary Key
            entity.HasKey(x => x.Id);

            // SKU
            entity.Property(x => x.SKU)
                .HasMaxLength(50)
                .IsRequired();

            // Unique SKU Index
            entity.HasIndex(x => x.SKU)
                .IsUnique();

            // Product Name
            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            // Category
            entity.Property(x => x.Category)
                .HasMaxLength(100)
                .IsRequired();

            // Description
            entity.Property(x => x.Description)
                .HasMaxLength(500);

            // Unit Price
            entity.Property(x => x.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            // Reorder Level
            entity.Property(x => x.ReorderLevel)
                .IsRequired();

            // Active Status
            entity.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            // Audit Fields
            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.Property(x => x.UpdatedAtUtc)
                .IsRequired(false);
        });
    }

    // Audit Field Handling
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();

        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();

        return base.SaveChanges();
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;

                // Preserve original CreatedAtUtc
                entry.Property(x => x.CreatedAtUtc).IsModified = false;
            }
        }
    }
}
