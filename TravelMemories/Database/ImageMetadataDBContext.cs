using Microsoft.EntityFrameworkCore;
using TravelMemories.Contracts.Data;
using TravelMemories.Contracts.Data.configs;

namespace TravelMemories.Database
{
    public class ImageMetadataDBContext : DbContext
    {
        public ImageMetadataDBContext(DbContextOptions<ImageMetadataDBContext> options) : base(options)
        {
        }

        public DbSet<ImageMetadata> ImageMetadata {  get; set; }

        public DbSet<UserInfo> UserInfo { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ImageMetadataConfig());
            modelBuilder.ApplyConfiguration(new UserInfoConfig());
        }
    }
}
