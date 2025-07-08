using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Configurations;

public class OrderItemsConfig : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(oi => oi.Quantity).IsRequired();
        builder.Property(oi => oi.PriceAtPurchase).IsRequired().HasColumnType("decimal(10, 2)");

        builder.HasOne(oi => oi.Order)
               .WithMany(oi => oi.OrderItems)
               .HasForeignKey(oi => oi.OrderId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.Book)
               .WithMany(oi => oi.OrderItems)
               .HasForeignKey(oi => oi.BookId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
    }
}
