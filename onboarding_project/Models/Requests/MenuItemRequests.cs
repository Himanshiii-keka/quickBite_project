using System.ComponentModel.DataAnnotations;

namespace startup_project.Models.Requests
{
    /// <summary>
    /// Request model for creating a new menu item.
    /// </summary>
    public class CreateMenuItemRequest
    {
        /// <example>Paneer Tikka</example>
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        /// <example>Grilled cottage cheese with spices</example>
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <example>180.00</example>
        [Range(0.01, 100000, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        /// <example>true</example>
        public bool IsAvailable { get; set; } = true;
    }

    /// <summary>
    /// Request model for updating menu item information.
    /// Partial update — only non-null fields are applied.
    /// </summary>
    public class UpdateMenuItemRequest
    {
        /// <example>Paneer Tikka Special</example>
        [MaxLength(150)]
        public string? Name { get; set; }

        /// <example>Grilled cottage cheese with extra spices</example>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <example>200.00</example>
        [Range(0.01, 100000, ErrorMessage = "Price must be greater than 0.")]
        public decimal? Price { get; set; }

        /// <example>true</example>
        public bool? IsAvailable { get; set; }
    }
}
