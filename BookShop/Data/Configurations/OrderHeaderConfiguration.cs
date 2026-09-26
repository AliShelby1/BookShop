using BookShop.Models;
using BookShop.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookShop.Data.Configurations
{
    public class OrderHeaderConfiguration : IEntityTypeConfiguration<OrderHeader>
    {
        public void Configure(EntityTypeBuilder<OrderHeader> builder)
        {
            builder.HasKey(o => o.Id);

            // Nullable ApplicationUserId for guest orders
            builder.Property(o => o.ApplicationUserId)
                .IsRequired(false);

            builder.HasOne(o => o.ApplicationUser)
                .WithMany()
                .HasForeignKey(o => o.ApplicationUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Index on ApplicationUserId for fast "My Orders" lookups
            builder.HasIndex(o => o.ApplicationUserId);

            // Index on CustomerEmail for guest lookup and email verification
            builder.HasIndex(o => o.CustomerEmail);

            // Unique index on OrderGuid for secure guest receipt links
            builder.HasIndex(o => o.OrderGuid)
                .IsUnique();

            // Index on OrderStatus for admin order management views
            builder.HasIndex(o => o.OrderStatus);

            // Index on OrderDate for date-range queries
            builder.HasIndex(o => o.OrderDate);

            // Store enum as int for performance
            builder.Property(o => o.OrderStatus)
                .HasConversion<int>();

            builder.Property(o => o.PaymentStatus)
                .HasConversion<int>();

            // One-to-many: OrderHeader has many OrderDetails
            builder.HasMany(o => o.OrderDetails)
                .WithOne(d => d.OrderHeader)
                .HasForeignKey(d => d.OrderHeaderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
