using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Models
{
    public class WishlistItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WishlistId { get; set; }

        [ForeignKey(nameof(WishlistId))]
        public Wishlist? Wishlist { get; set; }

        [Required]
        public int BookId { get; set; }

        [ForeignKey(nameof(BookId))]
        public Book? Book { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
