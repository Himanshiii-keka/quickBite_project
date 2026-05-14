using System.ComponentModel.DataAnnotations;

namespace startup_project.Models.ViewModels
{
    // ---------- Cart Item View Model ----------

    /// <summary>
    /// ViewModel for a single cart item.
    /// Represents cart item data without navigation to Cart or MenuItem entities.
    /// </summary>
    public class CartItemViewModel
    {
        public int MenuItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    // ---------- Cart View Model ----------

    /// <summary>
    /// ViewModel for displaying the user's cart.
    /// Contains restaurant reference and list of items without any navigation properties.
    /// </summary>
    public class CartViewModel
    {
        public int? RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }

    // ---------- Request Models ----------

    /// <summary>
    /// Request model for adding an item to cart.
    /// </summary>
    public class AddCartItemRequest
    {
        [Required]
        public int MenuItemId { get; set; }

        [Range(1, 99, ErrorMessage = "Quantity must be between 1 and 99.")]
        public int Quantity { get; set; } = 1;
    }
}
