using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookShop.Data;

namespace BookShop.Models
{
    public class Wishlist
    {
        [Key]
        public int Id { get; set; }

        public string? ApplicationUserId { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser? ApplicationUser { get; set; }

        /// <summary>
        /// Anonymous/guest wishlist identifier stored in browser cookie.
        /// When a guest logs in, their guest wishlist is merged into their ApplicationUser wishlist.
        /// </summary>
        [MaxLength(100)]
        public string? SessionWishlistId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
    }
}
