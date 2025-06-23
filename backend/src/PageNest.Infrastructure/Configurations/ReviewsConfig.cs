using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Configurations;

public class ReviewsConfig : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Comment).IsRequired(false).HasMaxLength(500);
        builder.Property<DateTime>("CreatedAt").HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(r => r.User)
               .WithMany(r => r.Reviews)
               .HasForeignKey(r => r.UserId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Book)
               .WithMany(r => r.Reviews)
               .HasForeignKey(r => r.BookId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.UserId, r.BookId }).IsUnique();
    }
}
