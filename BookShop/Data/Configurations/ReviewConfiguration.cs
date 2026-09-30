using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookShop.Models;

namespace BookShop.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Headline)
                .HasMaxLength(150)
                .IsRequired(false);

            builder.Property(r => r.Comment)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.IsVerifiedPurchase)
                .HasDefaultValue(true);

            builder.Property(r => r.IsApproved)
                .HasDefaultValue(true);

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            // Unique constraint: One review per customer per book
            builder.HasIndex(r => new { r.BookId, r.UserId })
                .IsUnique();

            // Composite index for fast approved review querying on the book details page
            builder.HasIndex(r => new { r.BookId, r.IsApproved, r.CreatedAt });

            // Relationships
            builder.HasOne(r => r.Book)
                .WithMany()
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
