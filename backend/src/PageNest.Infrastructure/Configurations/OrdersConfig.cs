using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Configurations;

public class OrdersConfig : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(o => o.Status).IsRequired();
        builder.Property(o => o.Total).IsRequired().HasColumnType("decimal(10, 2)");
        builder.Property<DateTime>("CreatedAt").HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(o => o.User)
               .WithMany(o => o.Orders)
               .HasForeignKey(o => o.UserId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.OrderItems)
               .WithOne(o => o.Order)
               .HasForeignKey(o => o.OrderId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
    }
}
