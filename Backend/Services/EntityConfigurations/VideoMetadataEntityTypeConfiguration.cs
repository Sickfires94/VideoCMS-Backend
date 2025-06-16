using Backend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Services.Configurations
{
    public class VideoMetadataEntityTypeConfiguration : IEntityTypeConfiguration<VideoMetadata>
    {
        public void Configure(EntityTypeBuilder<VideoMetadata> builder)
        {

            builder.HasKey(v => v.videoId);
            builder.Property(v => v.videoId)
                .ValueGeneratedOnAdd();

            builder.HasMany(v => v.videoTags)
                .WithMany();

            builder.HasOne(v => v.category)
                .WithMany();

            builder.HasOne(v => v.user)
                .WithMany();

            builder.Property(v => v.videoUploadDate)
                .HasDefaultValueSql("getdate()");
        }
    }
}
