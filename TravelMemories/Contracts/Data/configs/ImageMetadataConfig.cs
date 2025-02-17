using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelMemories.Contracts.Data.configs
{
    public class ImageMetadataConfig : IEntityTypeConfiguration<ImageMetadata>
    {
        public void Configure(EntityTypeBuilder<ImageMetadata> builder)
        {
            builder.ToTable("ImageMetadata");

            builder.HasKey(x => x.ID);

            builder.Property(x => x.Year).IsRequired();

            builder.Property(x => x.TripName).IsRequired();

            builder.Property(x => x.ImageName).IsRequired();

            builder.Property(x => x.X).IsRequired();

            builder.Property(x => x.Y).IsRequired();

            builder.HasOne(x => x.UploadedBy).WithMany(x => x.Images).HasForeignKey(x => x.UploadedByEmail).HasPrincipalKey(x => x.Email);
        }
    }
}
