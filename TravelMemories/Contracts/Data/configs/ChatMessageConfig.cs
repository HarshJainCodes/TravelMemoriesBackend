using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelMemoriesBackend.Contracts.Data;

namespace TravelMemories.Contracts.Data.configs
{
    public class ChatMessageConfig : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.ToTable("ChatMessages");

            builder.HasKey(x => x.MessageId);
            builder.Property(x => x.MessageId).IsRequired();

            builder.Property(x => x.ConversationId).IsRequired();

            builder.Property(x => x.Message).IsRequired();

            builder.Property(x => x.MessageRole).IsRequired();

            builder.Property(x => x.MessageRole).IsRequired();

            builder.HasOne(x => x.ChatConversation).WithMany(x => x.ConversationMessages).HasForeignKey(x => x.ConversationId).HasPrincipalKey(x => x.ConversationId);
        }
    }
}
