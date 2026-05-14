using System.ComponentModel.DataAnnotations;
using startup_project.Models.Enums;

namespace startup_project.Models.ViewModels
{
    // ---------- Order View Models ----------

    /// <summary>
    /// ViewModel for displaying a single order item in the response.
    /// Contains item-specific details without any navigation properties.
    /// </summary>
    public class OrderItemViewModel
    {
        public string ItemName { get; set; } = string.Empty;
        public decimal ItemPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    /// <summary>
    /// ViewModel for displaying order details.
    /// Flattens restaurant and user data into simple properties (no navigation properties).
    /// </summary>
    public class OrderViewModel
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderPlacedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemViewModel> Items { get; set; } = new();

        // Populated only for admin views (null when a user fetches their own orders)
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
    }

    // ---------- Request Models ----------

    /// <summary>
    /// Request model for updating order status.
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        /// <summary>1=Placed, 2=Confirmed, 3=Preparing, 4=OutForDelivery, 5=Delivered, 6=Cancelled</summary>
        [Required]
        public OrderStatus Status { get; set; }
    }
}
