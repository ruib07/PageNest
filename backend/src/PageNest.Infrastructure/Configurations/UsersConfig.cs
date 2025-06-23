using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Configurations;

public class UsersConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(u => u.Name).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(60);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Role).IsRequired();
        builder.Property<DateTime>("CreatedAt").HasDefaultValueSql("GETUTCDATE()");

        builder.HasMany(u => u.Orders)
               .WithOne(u => u.User)
               .HasForeignKey(u => u.UserId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
