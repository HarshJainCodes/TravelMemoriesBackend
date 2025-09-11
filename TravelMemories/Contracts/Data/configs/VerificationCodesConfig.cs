using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelMemories.Contracts.Data.configs
{
    public class VerificationCodesConfig : IEntityTypeConfiguration<VerificationCodes>
    {
        public void Configure(EntityTypeBuilder<VerificationCodes> builder)
        {
            builder.ToTable("VerificationCodes");

            builder.HasKey(x => x.UserEmail);

            builder.Property(x => x.OTP).IsRequired();

            builder.Property(x => x.IssuedAt).IsRequired();
        }
    }
}
