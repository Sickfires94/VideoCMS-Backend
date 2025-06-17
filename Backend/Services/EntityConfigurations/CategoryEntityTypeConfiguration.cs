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

            builder.HasOne(c => c.categoryParent)
               .WithMany()
               .IsRequired(false)
               .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
