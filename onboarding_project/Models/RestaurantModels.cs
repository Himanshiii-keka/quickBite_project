using System.ComponentModel.DataAnnotations;

namespace startup_project.Models
{
    // ---------- Response Models ----------

    public class RestaurantResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public bool IsActive { get; set; }
    }

    public class MenuItemResponse
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }

    // ---------- Admin Request Models ----------

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

    /// <summary>Partial update — only non-null fields are applied.</summary>
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
