using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Configurations;

public class RefreshTokensConfig : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(rt => rt.Token).IsRequired().HasMaxLength(255);
        builder.Property(rt => rt.ExpiresAt).IsRequired();
        builder.Property(rt => rt.IsRevoked).IsRequired().HasDefaultValue(false);
        builder.Property<DateTime>("CreatedAt").HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(rt => rt.User)
               .WithMany(rt => rt.RefreshTokens)
               .HasForeignKey(rt => rt.UserId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(rt => rt.Token).IsUnique();
    }
}
