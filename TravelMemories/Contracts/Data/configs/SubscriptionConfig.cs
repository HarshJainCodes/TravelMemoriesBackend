using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelMemoriesBackend.Contracts.Data;

namespace TravelMemories.Contracts.Data.configs
{
    public class SubscriptionConfig : IEntityTypeConfiguration<SubscriptionDetails>
    {
        public void Configure(EntityTypeBuilder<SubscriptionDetails> builder)
        {
            builder.ToTable("SubscriptionDetails");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.UserEmail).IsRequired();

            builder.HasIndex(x => x.UserEmail).IsUnique();

            builder.HasOne(x => x.User).WithOne(x => x.SubscriptionDetails).HasForeignKey<SubscriptionDetails>(x => x.UserEmail).HasPrincipalKey<UserInfo>(x => x.Email);
        }
    }
}
