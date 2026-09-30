using BookShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookShop.Data.Configurations
{
    public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
    {
        public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
        {
            builder.HasKey(t => t.Id);

            // Restrict deletion of Book if inventory history exists
            builder.HasOne(t => t.Book)
                .WithMany()
                .HasForeignKey(t => t.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            // Performance indexes for ledger filtering & reporting
            builder.HasIndex(t => t.BookId);
            builder.HasIndex(t => t.CreatedAt);
            builder.HasIndex(t => t.TransactionType);

            // Store enum as integer
            builder.Property(t => t.TransactionType)
                .HasConversion<int>();
        }
    }
}
