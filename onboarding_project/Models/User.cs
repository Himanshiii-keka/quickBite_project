using System.ComponentModel.DataAnnotations;
using startup_project.Models.Enums;

namespace startup_project.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(15)]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        public string HashedPassword { get; set; } = null!;

        public UserRole Role { get; set; } = UserRole.User;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
