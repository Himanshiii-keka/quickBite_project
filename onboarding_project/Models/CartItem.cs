using System.ComponentModel.DataAnnotations;

namespace startup_project.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public int CartId { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [Required]
        [Range(1, 99)]
        public int Quantity { get; set; }

        // Navigation
        public Cart Cart { get; set; } = null!;
        public MenuItem MenuItem { get; set; } = null!;
    }
}
