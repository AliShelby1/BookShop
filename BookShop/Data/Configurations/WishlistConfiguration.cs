using BookShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookShop.Data.Configurations
{
    public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
    {
        public void Configure(EntityTypeBuilder<Wishlist> builder)
        {
            builder.HasKey(w => w.Id);

            builder.Property(w => w.SessionWishlistId)
                .HasMaxLength(100);

            // Indexes for fast retrieval by user or guest session cookie
            builder.HasIndex(w => w.ApplicationUserId);
            builder.HasIndex(w => w.SessionWishlistId);

            // One Wishlist has Many WishlistItems; deleting the Wishlist cascades to its items
            builder.HasMany(w => w.Items)
                .WithOne(i => i.Wishlist)
                .HasForeignKey(i => i.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
