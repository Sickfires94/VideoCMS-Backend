using Backend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Services.EntityConfigurations
{
    public class VideoMetadataChangeLogEntityTypeConfiguration : IEntityTypeConfiguration<VideoMetadataChangeLog>
    {
        public void Configure(EntityTypeBuilder<VideoMetadataChangeLog> builder)
        {
            builder.HasKey(vc => new { vc.VideoId, vc.ChangeTime });
            builder.HasIndex(vc => vc.VideoId)
                .IsClustered();
        }

    }
}
