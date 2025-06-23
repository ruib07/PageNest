using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Configurations;

public class BooksConfig : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(b => b.Title).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Author).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Description).IsRequired().HasMaxLength(1000);
        builder.Property(b => b.PublishedDate).IsRequired();
        builder.Property(b => b.ISBN).IsRequired().HasMaxLength(20);
        builder.Property(b => b.PageCount).IsRequired();
        builder.Property(b => b.Language).IsRequired().HasMaxLength(3);
        builder.Property(b => b.CoverImageUrl).IsRequired().HasMaxLength(2048);
        builder.Property(b => b.Stock).IsRequired();
        builder.Property(b => b.Price).IsRequired().HasColumnType("decimal(10, 2)");

        builder.HasOne(b => b.Category)
               .WithMany(b => b.Books)
               .HasForeignKey(b => b.CategoryId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.BookGenres)
               .WithOne(b => b.Book)
               .HasForeignKey(b => b.BookId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Reviews)
               .WithOne(b => b.Book)
               .HasForeignKey(b => b.BookId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
    }
}
