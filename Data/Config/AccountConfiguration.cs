using Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Data.Config;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.UserType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(a => a.Password)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.VerificationCode)
            .HasMaxLength(100);

        builder.HasIndex(a => new { a.UserId, a.UserType }).IsUnique();

        builder.HasIndex(a => a.Email).IsUnique();
    }
}
