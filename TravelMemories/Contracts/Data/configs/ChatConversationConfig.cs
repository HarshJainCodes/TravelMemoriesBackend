using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelMemoriesBackend.Contracts.Data;

namespace TravelMemories.Contracts.Data.configs
{
    public class ChatConversationConfig : IEntityTypeConfiguration<ChatConversation>
    {
        public void Configure(EntityTypeBuilder<ChatConversation> builder)
        {
            builder.ToTable("ChatbotConversations");

            builder.HasKey(x => x.ConversationId);
            builder.Property(x => x.ConversationId).IsRequired();
            builder.Property(x => x.ConversationName).IsRequired();
            builder.Property(x => x.UserEmail).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();

            builder.HasOne(x => x.UserInfo).WithMany(x => x.ChatConversations).HasForeignKey(x => x.UserEmail).HasPrincipalKey(x => x.Email);
        }
    }
}
