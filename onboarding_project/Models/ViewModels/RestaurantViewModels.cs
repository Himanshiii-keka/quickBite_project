using System.ComponentModel.DataAnnotations;

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
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5.")]
        public decimal Rating { get; set; } = 0m;

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Request model for updating restaurant information.
    /// Partial update — only non-null fields are applied.
    /// </summary>
    public class UpdateRestaurantRequest
    {
        [MaxLength(150)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5.")]
        public decimal? Rating { get; set; }

        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Request model for creating a new menu item.
    /// </summary>
    public class CreateMenuItemRequest
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 100000, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        public bool IsAvailable { get; set; } = true;
    }

    /// <summary>
    /// Request model for updating menu item information.
    /// </summary>
    public class UpdateMenuItemRequest
    {
        [MaxLength(150)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(0.01, 100000, ErrorMessage = "Price must be greater than 0.")]
        public decimal? Price { get; set; }

        public bool? IsAvailable { get; set; }
    }
}
