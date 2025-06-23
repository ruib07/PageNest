using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Configurations;

public class BookGenresConfig : IEntityTypeConfiguration<BookGenre>
{
    public void Configure(EntityTypeBuilder<BookGenre> builder)
    {
        builder.ToTable("BookGenres");
        builder.HasKey(bg => new { bg.BookId, bg.GenreId });

        builder.HasOne(bg => bg.Book)
               .WithMany(bg => bg.BookGenres)
               .IsRequired()
               .HasForeignKey(bg => bg.BookId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bg => bg.Genre)
               .WithMany(bg => bg.BookGenres)
               .IsRequired()
               .HasForeignKey(bg => bg.GenreId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
