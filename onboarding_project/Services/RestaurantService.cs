using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using startup_project.Common;
using startup_project.Data;
using startup_project.Models;
using startup_project.Models.Common;
using startup_project.Models.ViewModels;

namespace startup_project.Services
{
    public class RestaurantService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private readonly ApplicationDbContext _db;
        private readonly IDistributedCache _cache;
        private readonly ILogger<RestaurantService> _logger;

        public RestaurantService(ApplicationDbContext db, IDistributedCache cache, ILogger<RestaurantService> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        // ---------- Browse (User-facing) ----------

        /// <summary>
        /// Returns only restaurants where IsActive = true.
        /// Cached with a 2-minute TTL. Redis failures are caught and logged; the request falls
        /// back to a fresh DB read so callers are never blocked by a cache outage.
        /// </summary>
        public async Task<List<RestaurantViewModel>> GetActiveAsync()
        {
            try
            {
                var cached = await _cache.GetStringAsync(PublicReadCache.ActiveRestaurantsKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    var fromCache = JsonSerializer.Deserialize<List<RestaurantViewModel>>(cached, JsonOptions);
                    if (fromCache != null) return fromCache;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache read failed for active restaurants; falling back to database.");
            }

            var fresh = await _db.Restaurants
                .AsNoTracking()
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .Select(r => new RestaurantViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    City = r.City,
                    Address = r.Address,
                    Rating = r.Rating,
                    IsActive = r.IsActive
                })
                .ToListAsync();

            try
            {
                await _cache.SetStringAsync(
                    PublicReadCache.ActiveRestaurantsKey,
                    JsonSerializer.Serialize(fresh, JsonOptions),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache write failed for active restaurants. DB data returned successfully.");
            }

            return fresh;
        }

        /// <summary>Returns ALL restaurants (active + inactive). Admin-only listing. No cache — admins need real-time data.</summary>
        public async Task<List<RestaurantViewModel>> GetAllAsync()
        {
            return await _db.Restaurants
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .Select(r => new RestaurantViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    City = r.City,
                    Address = r.Address,
                    Rating = r.Rating,
                    IsActive = r.IsActive
                })
                .ToListAsync();
        }

        public async Task<ServiceResult<RestaurantViewModel>> GetByIdAsync(int id, bool activeOnly)
        {
            var restaurant = await _db.Restaurants
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
                return ServiceResult<RestaurantViewModel>.Fail(StatusCodes.Status404NotFound, "Restaurant not found.");

            if (activeOnly && !restaurant.IsActive)
                return ServiceResult<RestaurantViewModel>.Fail(StatusCodes.Status404NotFound, "Restaurant not found.");

            return ServiceResult<RestaurantViewModel>.Ok(Map(restaurant));
        }

        // ---------- Admin: Create / Update ----------

        public async Task<ServiceResult<RestaurantViewModel>> CreateAsync(CreateRestaurantRequest request)
        {
            var restaurant = new Restaurant
            {
                Name = request.Name.Trim(),
                City = request.City.Trim(),
                Address = request.Address.Trim(),
                Rating = request.Rating,
                IsActive = request.IsActive
            };

            try
            {
                _db.Restaurants.Add(restaurant);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating restaurant '{Name}'.", request.Name);
                return ServiceResult<RestaurantViewModel>.Fail(StatusCodes.Status500InternalServerError,
                    "Failed to create restaurant due to a database error.");
            }

            try
            {
                await PublicReadCache.InvalidateActiveRestaurantsAsync(_cache);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache invalidation failed after creating restaurant {RestaurantId}.", restaurant.Id);
            }

            return ServiceResult<RestaurantViewModel>.Created(Map(restaurant), "Restaurant created.");
        }

        public async Task<ServiceResult<RestaurantViewModel>> UpdateAsync(int id, UpdateRestaurantRequest request)
        {
            var restaurant = await _db.Restaurants.FirstOrDefaultAsync(r => r.Id == id);
            if (restaurant == null)
                return ServiceResult<RestaurantViewModel>.Fail(StatusCodes.Status404NotFound, "Restaurant not found.");

            if (request.Name != null) restaurant.Name = request.Name.Trim();
            if (request.City != null) restaurant.City = request.City.Trim();
            if (request.Address != null) restaurant.Address = request.Address.Trim();
            if (request.Rating.HasValue) restaurant.Rating = request.Rating.Value;
            if (request.IsActive.HasValue) restaurant.IsActive = request.IsActive.Value;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating restaurant {RestaurantId}.", id);
                return ServiceResult<RestaurantViewModel>.Fail(StatusCodes.Status500InternalServerError,
                    "Failed to update restaurant due to a database error.");
            }

            try
            {
                await PublicReadCache.InvalidateActiveRestaurantsAsync(_cache);
                await PublicReadCache.InvalidateRestaurantMenusAsync(_cache, id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache invalidation failed after updating restaurant {RestaurantId}.", id);
            }

            return ServiceResult<RestaurantViewModel>.Ok(Map(restaurant), "Restaurant updated.");
        }

        private static RestaurantViewModel Map(Restaurant r) => new()
        {
            Id = r.Id,
            Name = r.Name,
            City = r.City,
            Address = r.Address,
            Rating = r.Rating,
            IsActive = r.IsActive
        };
    }
}
