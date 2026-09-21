using BookShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookShop.Data.Configurations
{
    public class StoreSettingConfiguration : IEntityTypeConfiguration<StoreSetting>
    {
        public void Configure(EntityTypeBuilder<StoreSetting> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.FreeShippingThreshold)
                .HasPrecision(18, 2);

            builder.Property(s => s.FlatShippingRate)
                .HasPrecision(18, 2);

            builder.Property(s => s.CartAnnouncementBanner)
                .HasMaxLength(250);

            // Seed default row
            builder.HasData(new StoreSetting
            {
                Id = 1,
                FreeShippingThreshold = 50.00m,
                FlatShippingRate = 4.99m,
                MaxQuantityPerBook = 10,
                CartAnnouncementBanner = "Complimentary literary bookmark & archival packaging on all orders over $50.",
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}
