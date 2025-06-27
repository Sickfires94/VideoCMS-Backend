using Backend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Services.Configurations
{
    public class TagEntityTypeConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {


            builder.HasKey(t => t.tagId);
            builder.Property(t => t.tagId)
                .ValueGeneratedOnAdd();

            builder.HasIndex(t => t.tagName)
                .IsUnique();

            builder.HasAlternateKey(t => t.tagName);
        }
    }
}
