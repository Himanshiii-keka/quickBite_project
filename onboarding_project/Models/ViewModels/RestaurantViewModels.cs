using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace startup_project.Models.ViewModels
{
    // ---------- Restaurant View Model ----------

    /// <summary>
    /// ViewModel for displaying restaurant information.
    /// No navigation properties to MenuItem or Order collections.
    /// </summary>
    public class RestaurantViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public bool IsActive { get; set; }
    }

    // ---------- Menu Item View Model ----------

    /// <summary>
    /// ViewModel for displaying menu item information.
    /// No navigation to Restaurant entity.
    /// </summary>
    public class MenuItemViewModel
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }

    // ---------- Admin Request Models ----------

    /// <summary>
    /// Request model for creating a new restaurant.
    /// </summary>
    public class CreateRestaurantRequest
    {
        /// <example>Spice Garden</example>
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        /// <example>Patna</example>
        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        /// <example>12 MG Road, Patna, Bihar 800001</example>
        [Required, MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        /// <example>4.2</example>
        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5.")]
        public decimal Rating { get; set; } = 0m;

        /// <example>true</example>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Request model for updating restaurant information.
    /// Partial update — only non-null fields are applied.
    /// </summary>
    public class UpdateRestaurantRequest
    {
        /// <example>Spice Garden Updated</example>
        [MaxLength(150)]
        public string? Name { get; set; }

        /// <example>Patna</example>
        [MaxLength(100)]
        public string? City { get; set; }

        /// <example>15 MG Road, Patna, Bihar 800001</example>
        [MaxLength(300)]
        public string? Address { get; set; }

        /// <example>4.5</example>
        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5.")]
        public decimal? Rating { get; set; }

        /// <example>true</example>
        public bool? IsActive { get; set; }
    }

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
