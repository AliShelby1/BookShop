using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookShop.Data;

namespace BookShop.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        public string? ApplicationUserId { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser? ApplicationUser { get; set; }

        /// <summary>
        /// Anonymous/guest shopping identifier stored in browser cookie.
        /// When a guest logs in, their guest cart is merged into their ApplicationUser cart.
        /// </summary>
        [MaxLength(100)]
        public string? SessionCartId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
