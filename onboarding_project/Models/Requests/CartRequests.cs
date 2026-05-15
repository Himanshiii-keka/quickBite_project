using System.ComponentModel.DataAnnotations;

namespace startup_project.Models.Requests
{
    /// <summary>
    /// Request model for adding an item to cart.
    /// </summary>
    public class AddCartItemRequest
    {
        /// <example>3</example>
        [Required]
        public int MenuItemId { get; set; }

        /// <example>2</example>
        [Range(1, 99, ErrorMessage = "Quantity must be between 1 and 99.")]
        public int Quantity { get; set; } = 1;
    }
}
