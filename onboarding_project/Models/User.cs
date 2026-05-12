using startup_project.Models.Enums;

namespace startup_project.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // Indexed for fast lookup during login and duplicate checks
        public string Email { get; set; } = string.Empty;

        // Indexed for fast lookup during duplicate phone checks
        public string PhoneNumber { get; set; } = string.Empty;

        public string HashedPassword { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation: one user can place many orders
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
