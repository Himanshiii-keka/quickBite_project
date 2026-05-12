namespace startup_project.Models
{
    public class Restaurant
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Stored as decimal to avoid floating point issues
        public decimal Rating { get; set; } = 0.0m;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation: one restaurant has many menu items and orders
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
