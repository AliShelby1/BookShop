using BookShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookShop.Data.Configurations
{
    public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
    {
        public void Configure(EntityTypeBuilder<WishlistItem> builder)
        {
            builder.HasKey(wi => wi.Id);

            // COMPOSITE UNIQUE INDEX: Ensures a book can only exist ONCE in any given wishlist
            // Mathematically prevents duplicates at the PostgreSQL database level
            builder.HasIndex(wi => new { wi.WishlistId, wi.BookId })
                .IsUnique();

            builder.HasOne(wi => wi.Book)
                .WithMany()
                .HasForeignKey(wi => wi.BookId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
