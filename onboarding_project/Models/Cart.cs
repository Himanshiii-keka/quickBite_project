using System.ComponentModel.DataAnnotations;

namespace startup_project.Models
{
    /// <summary>
    /// Each user has at most one cart. A cart can hold items from only ONE restaurant at a time.
    /// When the cart is emptied, RestaurantId becomes null again.
    /// </summary>
    public class Cart
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        // Null when cart is empty; set when first item is added
        public int? RestaurantId { get; set; }

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public Restaurant? Restaurant { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
