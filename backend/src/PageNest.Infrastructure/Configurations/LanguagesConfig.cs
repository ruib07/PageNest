using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Configurations;

public class LanguagesConfig : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("Languages");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(l => l.Name).IsRequired().HasMaxLength(100);
        builder.Property(l => l.Code).IsRequired().HasMaxLength(3);
        builder.Property(l => l.CultureCode).IsRequired().HasMaxLength(10);
    }
}
