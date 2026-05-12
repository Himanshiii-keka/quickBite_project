using System.ComponentModel.DataAnnotations;

namespace startup_project.Models
{
    // ---------- Request Models ----------

    public class AddCartItemRequest
    {
        [Required]
        public int MenuItemId { get; set; }

        [Range(1, 99, ErrorMessage = "Quantity must be between 1 and 99.")]
        public int Quantity { get; set; } = 1;
    }

    // ---------- Response Models ----------

    public class CartItemResponse
    {
        public int MenuItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class CartResponse
    {
        public int? RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public List<CartItemResponse> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }
}
