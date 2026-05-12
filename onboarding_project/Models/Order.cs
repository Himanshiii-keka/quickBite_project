using startup_project.Models.Enums;

namespace startup_project.Models
{
    public class Order
    {
        public int Id { get; set; }

        // FKs — indexed to speed up "orders by user" and "orders by restaurant" queries
        public int UserId { get; set; }
        public int RestaurantId { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Placed;

        public decimal TotalAmount { get; set; }

        public DateTime OrderPlacedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public Restaurant Restaurant { get; set; } = null!;

        // OrderItems are loaded with the order (joined)
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
