using Microsoft.EntityFrameworkCore;
using startup_project.Models;

namespace startup_project.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Restaurant> Restaurants => Set<Restaurant>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- User ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.PhoneNumber).IsUnique();
                entity.Property(u => u.Role).HasConversion<int>();
            });

            // --- Restaurant ---
            modelBuilder.Entity<Restaurant>(entity =>
            {
                entity.Property(r => r.Rating).HasColumnType("decimal(3,2)");
            });

            // --- MenuItem ---
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.Property(m => m.Price).HasColumnType("decimal(10,2)");
                entity.HasOne(m => m.Restaurant)
                      .WithMany(r => r.MenuItems)
                      .HasForeignKey(m => m.RestaurantId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- Order ---
            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.TotalAmount).HasColumnType("decimal(10,2)");
                entity.Property(o => o.Status).HasConversion<int>();
                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(o => o.Restaurant)
                      .WithMany(r => r.Orders)
                      .HasForeignKey(o => o.RestaurantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- OrderItem ---
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(oi => oi.ItemPrice).HasColumnType("decimal(10,2)");
                entity.Property(oi => oi.LineTotal).HasColumnType("decimal(10,2)");
                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(oi => oi.MenuItem)
                      .WithMany(m => m.OrderItems)
                      .HasForeignKey(oi => oi.MenuItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Cart ---
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasIndex(c => c.UserId).IsUnique();
                entity.HasOne(c => c.User)
                      .WithOne()
                      .HasForeignKey<Cart>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(c => c.Restaurant)
                      .WithMany()
                      .HasForeignKey(c => c.RestaurantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- CartItem ---
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasIndex(ci => new { ci.CartId, ci.MenuItemId }).IsUnique();
                entity.HasOne(ci => ci.Cart)
                      .WithMany(c => c.CartItems)
                      .HasForeignKey(ci => ci.CartId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ci => ci.MenuItem)
                      .WithMany()
                      .HasForeignKey(ci => ci.MenuItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
