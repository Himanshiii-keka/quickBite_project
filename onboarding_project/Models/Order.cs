using System.ComponentModel.DataAnnotations;
using startup_project.Models.Enums;

namespace startup_project.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int RestaurantId { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Placed;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        public DateTime OrderPlacedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public Restaurant Restaurant { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
