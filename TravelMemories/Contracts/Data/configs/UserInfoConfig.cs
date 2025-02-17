using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelMemories.Contracts.Data.configs
{
    public class UserInfoConfig : IEntityTypeConfiguration<UserInfo>
    {
        public void Configure(EntityTypeBuilder<UserInfo> builder)
        {
            builder.ToTable(nameof(UserInfo));

            builder.HasKey(x => x.UserID);
            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Email).IsRequired();

            // this will make email address unique such that no users will have the same address and it will make lookups faster
            builder.HasIndex(x => x.Email).IsUnique();
        }
    }
}
