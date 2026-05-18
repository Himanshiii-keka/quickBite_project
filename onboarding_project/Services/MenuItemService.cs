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
    public class MenuItemService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private readonly ApplicationDbContext _db;
        private readonly IDistributedCache _cache;
        private readonly ILogger<MenuItemService> _logger;

        public MenuItemService(ApplicationDbContext db, IDistributedCache cache, ILogger<MenuItemService> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        // ---------- Browse ----------

        /// <summary>
        /// Returns menu items for a restaurant. For users: restaurant must be active and only
        /// available items are returned. For admin: all items regardless of availability.
        /// Successful responses are cached per restaurant + filter mode (2 min TTL).
        /// Redis failures are caught and logged; the request falls back to a DB read.
        /// </summary>
        public async Task<ServiceResult<List<MenuItemViewModel>>> GetByRestaurantAsync(int restaurantId, bool availableOnly)
        {
            var restaurant = await _db.Restaurants
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == restaurantId);

            if (restaurant == null)
                return ServiceResult<List<MenuItemViewModel>>.Fail(StatusCodes.Status404NotFound, "Restaurant not found.");

            if (availableOnly && !restaurant.IsActive)
                return ServiceResult<List<MenuItemViewModel>>.Fail(StatusCodes.Status404NotFound, "Restaurant not found.");

            var cacheKey = PublicReadCache.MenuKey(restaurantId, availableOnly);

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    var fromCache = JsonSerializer.Deserialize<List<MenuItemViewModel>>(cached, JsonOptions);
                    if (fromCache != null)
                        return ServiceResult<List<MenuItemViewModel>>.Ok(fromCache);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache read failed for menu (restaurant {RestaurantId}); falling back to database.", restaurantId);
            }

            var query = _db.MenuItems.Where(m => m.RestaurantId == restaurantId);
            if (availableOnly)
                query = query.Where(m => m.IsAvailable);

            var items = await query
                .OrderBy(m => m.Name)
                .Select(m => new MenuItemViewModel
                {
                    Id = m.Id,
                    RestaurantId = m.RestaurantId,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    IsAvailable = m.IsAvailable
                })
                .ToListAsync();

            try
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(items, JsonOptions),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache write failed for menu (restaurant {RestaurantId}). DB data returned successfully.", restaurantId);
            }

            return ServiceResult<List<MenuItemViewModel>>.Ok(items);
        }

        // ---------- Admin: Create / Update / Delete ----------

        public async Task<ServiceResult<MenuItemViewModel>> CreateAsync(int restaurantId, CreateMenuItemRequest request)
        {
            bool restaurantExists = await _db.Restaurants.AnyAsync(r => r.Id == restaurantId);
            if (!restaurantExists)
                return ServiceResult<MenuItemViewModel>.Fail(StatusCodes.Status404NotFound, "Restaurant not found.");

            var item = new MenuItem
            {
                RestaurantId = restaurantId,
                Name = request.Name.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                Price = request.Price,
                IsAvailable = request.IsAvailable
            };

            try
            {
                _db.MenuItems.Add(item);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating menu item '{Name}' for restaurant {RestaurantId}.",
                    request.Name, restaurantId);
                return ServiceResult<MenuItemViewModel>.Fail(StatusCodes.Status500InternalServerError,
                    "Failed to create menu item due to a database error.");
            }

            try
            {
                await PublicReadCache.InvalidateRestaurantMenusAsync(_cache, restaurantId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache invalidation failed after creating menu item for restaurant {RestaurantId}.", restaurantId);
            }

            return ServiceResult<MenuItemViewModel>.Created(Map(item), "Menu item created.");
        }

        public async Task<ServiceResult<MenuItemViewModel>> UpdateAsync(int id, UpdateMenuItemRequest request)
        {
            var item = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
                return ServiceResult<MenuItemViewModel>.Fail(StatusCodes.Status404NotFound, "Menu item not found.");

            if (request.Name != null) item.Name = request.Name.Trim();
            if (request.Description != null) item.Description = request.Description.Trim();
            if (request.Price.HasValue) item.Price = request.Price.Value;
            if (request.IsAvailable.HasValue) item.IsAvailable = request.IsAvailable.Value;

            var restaurantId = item.RestaurantId;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating menu item {MenuItemId}.", id);
                return ServiceResult<MenuItemViewModel>.Fail(StatusCodes.Status500InternalServerError,
                    "Failed to update menu item due to a database error.");
            }

            try
            {
                await PublicReadCache.InvalidateRestaurantMenusAsync(_cache, restaurantId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache invalidation failed after updating menu item {MenuItemId}.", id);
            }

            return ServiceResult<MenuItemViewModel>.Ok(Map(item), "Menu item updated.");
        }

        /// <summary>
        /// Deletes a menu item. Rejected with 409 if the item appears in any historical order.
        ///
        /// Transaction note: this method removes rows from both CartItems and MenuItems.
        /// Both deletes are wrapped in an explicit transaction so a partial failure leaves the
        /// DB in a consistent state (either both are removed or neither is).
        /// </summary>
        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var item = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
                return ServiceResult.Fail(StatusCodes.Status404NotFound, "Menu item not found.");

            bool hasOrderHistory = await _db.OrderItems.AnyAsync(oi => oi.MenuItemId == id);
            if (hasOrderHistory)
                return ServiceResult.Fail(
                    StatusCodes.Status409Conflict,
                    "Cannot delete this menu item because it appears in existing orders. Set IsAvailable=false instead.");

            var restaurantId = item.RestaurantId;

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var cartReferences = _db.CartItems.Where(ci => ci.MenuItemId == id);
                _db.CartItems.RemoveRange(cartReferences);
                _db.MenuItems.Remove(item);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting menu item {MenuItemId}.", id);
                return ServiceResult.Fail(StatusCodes.Status500InternalServerError,
                    "Failed to delete menu item due to a database error.");
            }

            try
            {
                await PublicReadCache.InvalidateRestaurantMenusAsync(_cache, restaurantId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache invalidation failed after deleting menu item {MenuItemId}.", id);
            }

            return ServiceResult.Ok("Menu item deleted.");
        }

        private static MenuItemViewModel Map(MenuItem m) => new()
        {
            Id = m.Id,
            RestaurantId = m.RestaurantId,
            Name = m.Name,
            Description = m.Description,
            Price = m.Price,
            IsAvailable = m.IsAvailable
        };
    }
}
