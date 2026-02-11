using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelMemoriesBackend.Contracts.Data;

namespace TravelMemories.Contracts.Data.configs
{
    public class OAuthCodeStoreConfig : IEntityTypeConfiguration<OAuthCodeStore>
    {
        public void Configure(EntityTypeBuilder<OAuthCodeStore> builder)
        {
            builder.ToTable("OAuthCodeStores");

            builder.HasKey(p => p.Code);

            builder.Property(p => p.Code).IsRequired();

            builder.Property(p => p.LoginChallenge).IsRequired();

            builder.Property(p => p.Email).IsRequired();

            builder.HasIndex(p => p.Code).IsUnique();
        }
    }
}
