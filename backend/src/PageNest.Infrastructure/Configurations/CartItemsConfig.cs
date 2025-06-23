using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Configurations;

public class CartItemsConfig : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        builder.HasKey(ci => ci.Id);
        builder.Property(ci => ci.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(ci => ci.Quantity).IsRequired();

        builder.HasOne(ci => ci.User)
               .WithMany(ci => ci.CartItems)
               .HasForeignKey(ci => ci.UserId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.Book)
               .WithMany(ci => ci.CartItems)
               .HasForeignKey(ci => ci.BookId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
    }
}
