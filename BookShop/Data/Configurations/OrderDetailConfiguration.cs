using BookShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookShop.Data.Configurations
{
    public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
    {
        public void Configure(EntityTypeBuilder<OrderDetail> builder)
        {
            builder.HasKey(d => d.Id);

            // Book reference uses Restrict (not Cascade) so that deleting a book
            // does not destroy historical order records. The BookTitle snapshot
            // on the row itself preserves the receipt data permanently.
            builder.HasOne(d => d.Book)
                .WithMany()
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
