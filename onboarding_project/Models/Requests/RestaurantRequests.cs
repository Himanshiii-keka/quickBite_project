using System.ComponentModel.DataAnnotations;

namespace startup_project.Models.Requests
{
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
}
