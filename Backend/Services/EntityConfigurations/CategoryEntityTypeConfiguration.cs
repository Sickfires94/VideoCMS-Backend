using Backend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Services.Configurations
{
    public class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {

            builder.HasKey(c => c.categoryId);
            builder.Property(c => c.categoryId)
                .ValueGeneratedOnAdd();

            builder.HasAlternateKey(c => c.categoryName);

            builder.HasIndex(c => c.categoryParentId)
                .IsClustered();

            builder.HasIndex(c => c.categoryName)
                .IsUnique();

            builder.HasOne(c => c.categoryParent)
               .WithMany(c => c.children)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
