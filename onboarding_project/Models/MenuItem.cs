using System.ComponentModel.DataAnnotations;

namespace startup_project.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required]
        public int RestaurantId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string Description { get; set; } = null!;

        [Required]
        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation
        public Restaurant Restaurant { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
