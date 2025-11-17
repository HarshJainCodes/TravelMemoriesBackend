using Microsoft.EntityFrameworkCore;
using TravelMemories.Contracts.Data;
using TravelMemories.Contracts.Data.configs;
using TravelMemoriesBackend.Contracts.Data;

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

        public DbSet<SubscriptionDetails> SubscriptionDetails { get; set; }

        public  DbSet<ChatConversation> ChatbotConversations { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ImageMetadataConfig());
            modelBuilder.ApplyConfiguration(new UserInfoConfig());
            modelBuilder.ApplyConfiguration(new VerificationCodesConfig());
            modelBuilder.ApplyConfiguration(new SubscriptionConfig());
            modelBuilder.ApplyConfiguration(new ChatConversationConfig());
            modelBuilder.ApplyConfiguration(new ChatMessageConfig());
        }
    }
}
