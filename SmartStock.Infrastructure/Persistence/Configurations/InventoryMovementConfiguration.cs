using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartStock.Domain.Entities;

namespace SmartStock.Infrastructure.Persistence.Configurations
{
    public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
    {
        public void Configure(EntityTypeBuilder<InventoryMovement> builder)
        {
            builder.ToTable("InventoryMovements");

            builder.Property(x => x.Quantity)
                .HasPrecision(18, 3);

            builder.Property(x => x.QuantityBefore)
                .HasPrecision(18, 3);

            builder.Property(x => x.QuantityAfter)
                .HasPrecision(18, 3);

            builder.Property(x => x.MovementType)
                .HasConversion<int>();

            builder.Property(x => x.Remarks)
                .HasMaxLength(500);

            builder.HasOne(x => x.Inventory)
                .WithMany(x => x.Movements)
                .HasForeignKey(x => x.InventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}