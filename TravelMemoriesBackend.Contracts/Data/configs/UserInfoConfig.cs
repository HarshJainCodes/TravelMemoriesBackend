using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelMemoriesBackend.Contracts.Data.configs
{
    public class UserInfoConfig : IEntityTypeConfiguration<UserInfo>
    {
        public void Configure(EntityTypeBuilder<UserInfo> builder)
        {
            builder.ToTable(nameof(UserInfo));

            builder.HasKey(x => x.UserID);
            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Email).IsRequired();
            builder.Property(x => x.ProfilePictureURL).HasDefaultValue("");
            builder.Property(x => x.IsManualLogin).HasDefaultValue(false);
            builder.Property(x => x.Password).HasDefaultValue("");

            // this will make email address unique such that no users will have the same address and it will make lookups faster
            builder.HasIndex(x => x.Email).IsUnique();
        }
    }
}
