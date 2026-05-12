using System.ComponentModel.DataAnnotations;
using startup_project.Models.Enums;

namespace startup_project.Models
{
    // ---------- Request Models ----------

    public class UpdateOrderStatusRequest
    {
        /// <summary>1=Placed, 2=Confirmed, 3=Preparing, 4=OutForDelivery, 5=Delivered, 6=Cancelled</summary>
        [Required]
        public OrderStatus Status { get; set; }
    }

    // ---------- Response Models ----------

    public class OrderItemResponse
    {
        public string ItemName { get; set; } = string.Empty;
        public decimal ItemPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class OrderResponse
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderPlacedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();

        // Populated only for admin views (null when a user fetches their own orders)
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
    }
}
