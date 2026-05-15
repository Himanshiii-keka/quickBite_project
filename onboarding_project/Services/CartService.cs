using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using startup_project.Common;
using startup_project.Data;
using startup_project.Models;
using startup_project.Models.Common;
using startup_project.Models.Requests;
using startup_project.Models.ViewModels;

namespace startup_project.Services
{
    public class CartService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private readonly ApplicationDbContext _db;
        private readonly IDistributedCache _cache;
        private readonly ILogger<CartService> _logger;

        public CartService(ApplicationDbContext db, IDistributedCache cache, ILogger<CartService> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        // ---------- View Cart ----------

        /// <summary>
        /// Returns the user's cart. Tries the distributed cache first (60 s TTL) so repeated
        /// GET /cart calls don't hit SQL Server on every request. Falls back to DB on cache miss
        /// or if Redis is unavailable. Cart data itself always lives in DB for checkout consistency.
        /// Each table is queried separately — no SQL JOINs.
        /// </summary>
        public async Task<CartViewModel> GetCartAsync(int userId)
        {
            var cacheKey = PublicReadCache.CartKey(userId);

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    var fromCache = JsonSerializer.Deserialize<CartViewModel>(cached, JsonOptions);
                    if (fromCache != null) return fromCache;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache read failed for cart (user {UserId}); falling back to database.", userId);
            }

            var viewModel = await LoadAndBuildCartViewAsync(userId);

            try
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(viewModel, JsonOptions),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache write failed for cart (user {UserId}). DB data returned successfully.", userId);
            }

            return viewModel;
        }

        // ---------- Add Item ----------

        /// <summary>
        /// Adds a menu item to the user's cart. Enforces "one restaurant per cart":
        /// if the cart already holds items from a different restaurant, returns 409.
        /// If the item is already in the cart, its quantity is incremented instead of adding a duplicate row.
        /// Each table is queried separately — no SQL JOINs.
        /// </summary>
        public async Task<ServiceResult<CartViewModel>> AddItemAsync(int userId, AddCartItemRequest request)
        {
            var menuItem = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == request.MenuItemId);

            if (menuItem == null)
                return ServiceResult<CartViewModel>.Fail(StatusCodes.Status404NotFound, "Menu item not found.");

            if (!menuItem.IsAvailable)
                return ServiceResult<CartViewModel>.Fail(StatusCodes.Status400BadRequest, "This menu item is currently unavailable.");

            var restaurant = await _db.Restaurants.FirstOrDefaultAsync(r => r.Id == menuItem.RestaurantId);

            if (restaurant == null || !restaurant.IsActive)
                return ServiceResult<CartViewModel>.Fail(StatusCodes.Status400BadRequest, "This restaurant is currently inactive.");

            // Check the restaurant conflict BEFORE opening a transaction to avoid unnecessary round-trips.
            var existingCart = await _db.Carts.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId);
            if (existingCart?.RestaurantId.HasValue == true && existingCart.RestaurantId.Value != menuItem.RestaurantId)
            {
                return ServiceResult<CartViewModel>.Fail(
                    StatusCodes.Status409Conflict,
                    "Your cart already contains items from another restaurant. Please clear it before ordering from a different restaurant.");
            }

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var cart = await LoadOrCreateCartAsync(userId);
                var cartItems = await _db.CartItems.Where(ci => ci.CartId == cart.Id).ToListAsync();

                if (!cart.RestaurantId.HasValue)
                    cart.RestaurantId = menuItem.RestaurantId;

                var existing = cartItems.FirstOrDefault(ci => ci.MenuItemId == request.MenuItemId);
                if (existing != null)
                {
                    existing.Quantity += request.Quantity;
                }
                else
                {
                    _db.CartItems.Add(new CartItem
                    {
                        CartId = cart.Id,
                        MenuItemId = menuItem.Id,
                        Quantity = request.Quantity
                    });
                }

                cart.UpdatedAtUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error adding item {MenuItemId} to cart for user {UserId}.",
                    request.MenuItemId, userId);
                return ServiceResult<CartViewModel>.Fail(StatusCodes.Status500InternalServerError,
                    "Failed to add item to cart.");
            }

            await InvalidateCartCacheAsync(userId);
            return ServiceResult<CartViewModel>.Ok(await LoadAndBuildCartViewAsync(userId), "Item added to cart.");
        }

        // ---------- Remove Item ----------

        public async Task<ServiceResult<CartViewModel>> RemoveItemAsync(int userId, int menuItemId)
        {
            var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
                return ServiceResult<CartViewModel>.Fail(StatusCodes.Status404NotFound, "Your cart is empty.");

            var cartItems = await _db.CartItems.Where(ci => ci.CartId == cart.Id).ToListAsync();
            if (cartItems.Count == 0)
                return ServiceResult<CartViewModel>.Fail(StatusCodes.Status404NotFound, "Your cart is empty.");

            var line = cartItems.FirstOrDefault(ci => ci.MenuItemId == menuItemId);
            if (line == null)
                return ServiceResult<CartViewModel>.Fail(StatusCodes.Status404NotFound, "Item not found in cart.");

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.CartItems.Remove(line);

                if (cartItems.Count == 1)
                    cart.RestaurantId = null;

                cart.UpdatedAtUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error removing item {MenuItemId} from cart for user {UserId}.",
                    menuItemId, userId);
                return ServiceResult<CartViewModel>.Fail(StatusCodes.Status500InternalServerError,
                    "Failed to remove item from cart.");
            }

            await InvalidateCartCacheAsync(userId);
            return ServiceResult<CartViewModel>.Ok(await LoadAndBuildCartViewAsync(userId), "Item removed from cart.");
        }

        // ---------- Clear Cart ----------

        public async Task<ServiceResult> ClearAsync(int userId)
        {
            var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
                return ServiceResult.Ok("Cart is already empty.");

            var cartItems = await _db.CartItems.Where(ci => ci.CartId == cart.Id).ToListAsync();

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.CartItems.RemoveRange(cartItems);
                cart.RestaurantId = null;
                cart.UpdatedAtUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error clearing cart for user {UserId}.", userId);
                return ServiceResult.Fail(StatusCodes.Status500InternalServerError, "Failed to clear cart.");
            }

            await InvalidateCartCacheAsync(userId);
            return ServiceResult.Ok("Cart cleared.");
        }

        // ---------- Private Helpers ----------

        /// <summary>
        /// Builds a CartViewModel by loading each table separately:
        /// Cart → CartItems → MenuItems (batch) → Restaurant.
        /// No SQL JOINs.
        /// </summary>
        private async Task<CartViewModel> LoadAndBuildCartViewAsync(int userId)
        {
            var cart = await _db.Carts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return new CartViewModel();

            var cartItems = await _db.CartItems
                .AsNoTracking()
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            if (cartItems.Count == 0) return new CartViewModel();

            var menuItemIds = cartItems.Select(ci => ci.MenuItemId).ToList();
            var menuItems = await _db.MenuItems
                .AsNoTracking()
                .Where(m => menuItemIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id);

            string? restaurantName = null;
            if (cart.RestaurantId.HasValue)
            {
                restaurantName = await _db.Restaurants
                    .AsNoTracking()
                    .Where(r => r.Id == cart.RestaurantId.Value)
                    .Select(r => r.Name)
                    .FirstOrDefaultAsync();
            }

            var response = new CartViewModel
            {
                RestaurantId = cart.RestaurantId,
                RestaurantName = restaurantName,
                Items = cartItems.Select(ci =>
                {
                    var mi = menuItems[ci.MenuItemId];
                    return new CartItemViewModel
                    {
                        MenuItemId = ci.MenuItemId,
                        ItemName = mi.Name,
                        UnitPrice = mi.Price,
                        Quantity = ci.Quantity,
                        LineTotal = mi.Price * ci.Quantity
                    };
                }).ToList()
            };

            response.TotalAmount = response.Items.Sum(i => i.LineTotal);
            return response;
        }

        private async Task<Cart> LoadOrCreateCartAsync(int userId)
        {
            var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart != null) return cart;

            cart = new Cart { UserId = userId };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
            return cart;
        }

        private async Task InvalidateCartCacheAsync(int userId)
        {
            try
            {
                await PublicReadCache.InvalidateCartAsync(_cache, userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache invalidation failed for cart (user {UserId}). " +
                    "Stale cache will expire within the TTL window.", userId);
            }
        }
    }
}
