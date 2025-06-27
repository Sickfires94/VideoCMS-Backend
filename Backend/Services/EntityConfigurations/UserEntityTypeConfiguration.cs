using Backend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace Backend.Services.Configurations
{
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.userId);
            builder.Property(u => u.userId)
                .ValueGeneratedOnAdd();

            builder.HasIndex(u => u.userId)
                .IsUnique();

            builder.HasAlternateKey(u => u.userName);

            builder.HasAlternateKey(u => u.userEmail);

            builder.Property(u => u.userCreatedDate)
            .HasDefaultValueSql("getdate()");

            builder.Property(u => u.userUpdatedDate)
                .HasDefaultValueSql("getDate()");
        }
    }
}
