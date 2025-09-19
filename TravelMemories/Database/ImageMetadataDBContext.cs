using Microsoft.EntityFrameworkCore;
using TravelMemoriesBackend.Contracts.Data;
using TravelMemoriesBackend.Contracts.Data.configs;

namespace TravelMemories.Database
{
    public class ImageMetadataDBContext : DbContext
    {
        public ImageMetadataDBContext(DbContextOptions<ImageMetadataDBContext> options) : base(options)
        {
        }

        public DbSet<ImageMetadata> ImageMetadata {  get; set; }

        public DbSet<UserInfo> UserInfo { get; set; }

        public DbSet<VerificationCodes> VerificationCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ImageMetadataConfig());
            modelBuilder.ApplyConfiguration(new UserInfoConfig());
            modelBuilder.ApplyConfiguration(new VerificationCodesConfig());
        }
    }
}
