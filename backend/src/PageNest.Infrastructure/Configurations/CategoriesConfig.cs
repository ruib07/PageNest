using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Configurations;

public class CategoriesConfig : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);

        builder.HasMany(c => c.Books)
               .WithOne(c => c.Category)
               .HasForeignKey(c => c.CategoryId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.Name).IsUnique();
    }
}
