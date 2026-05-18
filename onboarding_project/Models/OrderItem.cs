using System.ComponentModel.DataAnnotations;

namespace startup_project.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [Required]
        [MaxLength(150)]
        public string ItemName { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal ItemPrice { get; set; }

        [Required]
        [Range(1, 99)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal LineTotal { get; set; }
    }
}
