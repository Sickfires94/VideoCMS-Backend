using Backend.DTOs;
using Backend.Services.Configurations;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace Backend.Services
{
    public class VideoManagementApplicationContext : DbContext
    {
        public VideoManagementApplicationContext(DbContextOptions<VideoManagementApplicationContext> options) : base(options) { }

        public DbSet<Category> categories { get; set; }
        public DbSet<Tag> tags { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<VideoMetadata> videoMetadatas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            new UserEntityTypeConfiguration().Configure(modelBuilder.Entity<User>());
            new CategoryEntityTypeConfiguration().Configure(modelBuilder.Entity<Category>());
            new TagEntityTypeConfiguration().Configure(modelBuilder.Entity<Tag>());
            new VideoMetadataEntityTypeConfiguration().Configure(modelBuilder.Entity<VideoMetadata>());

        }
    }
}
