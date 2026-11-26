using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartStock.Domain.Entities;

namespace SmartStock.Infrastructure.Persistence.Configurations
{
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            builder.ToTable("Inventories");

            builder.Property(x => x.Quantity)
                .HasPrecision(18, 3);

            builder.Property(x => x.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            builder.HasIndex(x => x.ProductId)
                .IsUnique();

            builder.HasOne(x => x.Product)
                .WithOne(x => x.Inventory)
                .HasForeignKey<Inventory>(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}