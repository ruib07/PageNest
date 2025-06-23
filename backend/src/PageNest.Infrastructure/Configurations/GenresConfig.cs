using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Configurations;

public class GenresConfig : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.ToTable("Genres");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(g => g.Name).IsRequired().HasMaxLength(100);

        builder.HasMany(g => g.BookGenres)
               .WithOne(g => g.Genre)
               .HasForeignKey(g => g.GenreId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(g => g.Name).IsUnique();
    }
}
