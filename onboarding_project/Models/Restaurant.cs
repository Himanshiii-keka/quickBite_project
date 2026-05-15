using System.ComponentModel.DataAnnotations;

namespace startup_project.Models
{
    public class Restaurant
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = null!;

        [Required]
        [MaxLength(300)]
        public string Address { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        [Range(0, 5)]
        public decimal Rating { get; set; } = 0.0m;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
