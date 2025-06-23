using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PageNest.Domain.Entities;

namespace PageNest.Infrastructure.Configurations;

public class PasswordResetsConfig : IEntityTypeConfiguration<PasswordReset>
{
    public void Configure(EntityTypeBuilder<PasswordReset> builder)
    {
        builder.ToTable("PasswordResets");
        builder.HasKey(pr => pr.Id);
        builder.Property(pr => pr.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(pr => pr.Token).IsRequired().HasMaxLength(255);
        builder.Property(pr => pr.ExpirationDate).IsRequired();

        builder.HasOne(pr => pr.User)
               .WithMany(pr => pr.PasswordResets)
               .HasForeignKey(pr => pr.UserId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
    }
}
