using Microsoft.EntityFrameworkCore;
using startup_project.Data;
using startup_project.Models;
using startup_project.Models.Common;
using startup_project.Models.ViewModels;

namespace startup_project.Services
{
    public class CartService
    {
        private readonly ApplicationDbContext _db;

        public CartService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ---------- View Cart ----------

        public async Task<CartViewModel> GetCartAsync(int userId)
        {
            var cart = await LoadCartAsync(userId);
            return BuildResponse(cart);
        }

        // ---------- Add Item ----------

        /// <summary>
        /// Adds a menu item to the user's cart. Enforces "one restaurant per cart":
        /// if the cart already holds items from a different restaurant, returns 409 with a clear message.
        /// If the item is already in the cart, its quantity is incremented instead of adding a duplicate row.
        /// </summary>
        public async Task<ServiceResult<CartViewModel>> AddItemAsync(int userId, AddCartItemRequest request)
        {
            var menuItem = await _db.MenuItems
                .Include(m => m.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == request.MenuItemId);

            if (menuItem == null)
                return ServiceResult<CartViewModel>.Fail(StatusCodes.Status404NotFound, "Menu item not found.");

            if (!menuItem.IsAvailable)
                return ServiceResult<CartViewModel>.Fail(StatusCodes.Status400BadRequest, "This menu item is currently unavailable.");

            if (!menuItem.Restaurant.IsActive)
                return ServiceResult<CartViewModel>.Fail(StatusCodes.Status400BadRequest, "This restaurant is currently inactive.");

            var cart = await LoadOrCreateCartAsync(userId);

            // Business rule: cart can only hold items from one restaurant at a time
            if (cart.RestaurantId.HasValue && cart.RestaurantId.Value != menuItem.RestaurantId)
            {
                return ServiceResult<CartViewModel>.Fail(
                    StatusCodes.Status409Conflict,
                    "Your cart already contains items from another restaurant. Please clear it before ordering from a different restaurant.");
            }

            // Lock the cart to this restaurant on first add
            if (!cart.RestaurantId.HasValue)
                cart.RestaurantId = menuItem.RestaurantId;

            // Merge with existing row if already in cart, else insert
            var existing = cart.CartItems.FirstOrDefault(ci => ci.MenuItemId == request.MenuItemId);
            if (existing != null)
            {
                existing.Quantity += request.Quantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    MenuItemId = menuItem.Id,
                    Quantity = request.Quantity
                });
            }

            cart.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Reload to get fresh MenuItem navigation for response
            var refreshed = await LoadCartAsync(userId);
            return ServiceResult<CartViewModel>.Ok(BuildResponse(refreshed), "Item added to cart.");
        }

        // ---------- Remove Item ----------

        public async Task<ServiceResult<CartViewModel>> RemoveItemAsync(int userId, int menuItemId)
        {
            var cart = await LoadCartAsync(userId);
            if (cart == null || cart.CartItems.Count == 0)
                return ServiceResult<CartViewModel>.Fail(StatusCodes.Status404NotFound, "Your cart is empty.");

            var line = cart.CartItems.FirstOrDefault(ci => ci.MenuItemId == menuItemId);
            if (line == null)
                return ServiceResult<CartViewModel>.Fail(StatusCodes.Status404NotFound, "Item not found in cart.");

            _db.CartItems.Remove(line);

            // If that was the last item, also clear the restaurant binding so the user can
            // order from a different restaurant on the next add.
            if (cart.CartItems.Count == 1)
                cart.RestaurantId = null;

            cart.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var refreshed = await LoadCartAsync(userId);
            return ServiceResult<CartViewModel>.Ok(BuildResponse(refreshed), "Item removed from cart.");
        }

        // ---------- Clear Cart ----------

        public async Task<ServiceResult> ClearAsync(int userId)
        {
            var cart = await LoadCartAsync(userId);
            if (cart == null)
                return ServiceResult.Ok("Cart is already empty.");

            _db.CartItems.RemoveRange(cart.CartItems);
            cart.RestaurantId = null;
            cart.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return ServiceResult.Ok("Cart cleared.");
        }

        // ---------- Helpers ----------

        // EF tracked load — used when we plan to modify the cart
        private Task<Cart?> LoadCartAsync(int userId)
        {
            return _db.Carts
                .Include(c => c.Restaurant)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.MenuItem)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        private async Task<Cart> LoadOrCreateCartAsync(int userId)
        {
            var cart = await LoadCartAsync(userId);
            if (cart != null) return cart;

            cart = new Cart { UserId = userId };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
            return cart;
        }

        private static CartViewModel BuildResponse(Cart? cart)
        {
            if (cart == null || cart.CartItems.Count == 0)
                return new CartViewModel();

            var response = new CartViewModel
            {
                RestaurantId = cart.RestaurantId,
                RestaurantName = cart.Restaurant?.Name,
                Items = cart.CartItems
                    .Select(ci => new CartItemViewModel
                    {
                        MenuItemId = ci.MenuItemId,
                        ItemName = ci.MenuItem.Name,
                        UnitPrice = ci.MenuItem.Price,
                        Quantity = ci.Quantity,
                        LineTotal = ci.MenuItem.Price * ci.Quantity
                    })
                    .ToList()
            };

            response.TotalAmount = response.Items.Sum(i => i.LineTotal);
            return response;
        }
    }
}
