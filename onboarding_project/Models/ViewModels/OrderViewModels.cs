using System.ComponentModel.DataAnnotations;
using startup_project.Models.Enums;

namespace startup_project.Models.ViewModels
{
    // ---------- Shared Item Line ----------

    public class OrderItemViewModel
    {
        public string ItemName { get; set; } = string.Empty;
        public decimal ItemPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    // ---------- User-Facing Order View ----------

    /// <summary>Order details returned to the placing user. Contains no customer identity fields.</summary>
    public class UserOrderViewModel
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderPlacedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemViewModel> Items { get; set; } = new();
    }

    // ---------- Admin-Facing Order View ----------

    /// <summary>Order details returned to admins — includes customer identity on top of the base view.</summary>
    public class AdminOrderViewModel : UserOrderViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
    }

    // ---------- Request Models ----------

    public class UpdateOrderStatusRequest
    {
        /// <summary>1=Placed, 2=Confirmed, 3=Preparing, 4=OutForDelivery, 5=Delivered, 6=Cancelled</summary>
        [Required]
        public OrderStatus Status { get; set; }
    }
}
