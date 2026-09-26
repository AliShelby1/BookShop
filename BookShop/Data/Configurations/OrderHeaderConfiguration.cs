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

            // Index on ApplicationUserId for fast "My Orders" lookups
            builder.HasIndex(o => o.ApplicationUserId);

            // Index on OrderStatus for admin order management views
            builder.HasIndex(o => o.OrderStatus);

            // Index on OrderDate for date-range queries
            builder.HasIndex(o => o.OrderDate);

            // Store enum as int for performance (not string)
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
