using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using startup_project.Common;
using startup_project.Data;
using startup_project.Models;
using startup_project.Models.Common;

namespace startup_project.Services
{
    public class RestaurantService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private readonly ApplicationDbContext _db;
        private readonly IDistributedCache _cache;

        public RestaurantService(ApplicationDbContext db, IDistributedCache cache)
        {
            _db = db;
            _cache = cache;
        }

        // ---------- Browse (User-facing) ----------

        /// <summary>Returns only restaurants where IsActive = true. Cached (Redis or memory) with a short TTL.</summary>
        public async Task<List<RestaurantResponse>> GetActiveAsync()
        {
            var cached = await _cache.GetStringAsync(PublicReadCache.ActiveRestaurantsKey);
            if (!string.IsNullOrEmpty(cached))
            {
                var fromCache = JsonSerializer.Deserialize<List<RestaurantResponse>>(cached, JsonOptions);
                if (fromCache != null)
                    return fromCache;
            }

            var fresh = await _db.Restaurants
                .AsNoTracking()
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .Select(r => new RestaurantResponse
                {
                    Id = r.Id,
                    Name = r.Name,
                    City = r.City,
                    Address = r.Address,
                    Rating = r.Rating,
                    IsActive = r.IsActive
                })
                .ToListAsync();

            await _cache.SetStringAsync(
                PublicReadCache.ActiveRestaurantsKey,
                JsonSerializer.Serialize(fresh, JsonOptions),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) });

            return fresh;
        }

        /// <summary>Returns ALL restaurants (active + inactive). Admin-only listing.</summary>
        public async Task<List<RestaurantResponse>> GetAllAsync()
        {
            return await _db.Restaurants
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .Select(r => new RestaurantResponse
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

        public async Task<ServiceResult<RestaurantResponse>> GetByIdAsync(int id, bool activeOnly)
        {
            var restaurant = await _db.Restaurants
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
                return ServiceResult<RestaurantResponse>.Fail(StatusCodes.Status404NotFound, "Restaurant not found.");

            if (activeOnly && !restaurant.IsActive)
                return ServiceResult<RestaurantResponse>.Fail(StatusCodes.Status404NotFound, "Restaurant not found.");

            return ServiceResult<RestaurantResponse>.Ok(Map(restaurant));
        }

        // ---------- Admin: Create / Update ----------

        public async Task<ServiceResult<RestaurantResponse>> CreateAsync(CreateRestaurantRequest request)
        {
            var restaurant = new Restaurant
            {
                Name = request.Name.Trim(),
                City = request.City.Trim(),
                Address = request.Address.Trim(),
                Rating = request.Rating,
                IsActive = request.IsActive
            };

            _db.Restaurants.Add(restaurant);
            await _db.SaveChangesAsync();

            await PublicReadCache.InvalidateActiveRestaurantsAsync(_cache);

            return ServiceResult<RestaurantResponse>.Created(Map(restaurant), "Restaurant created.");
        }

        public async Task<ServiceResult<RestaurantResponse>> UpdateAsync(int id, UpdateRestaurantRequest request)
        {
            var restaurant = await _db.Restaurants.FirstOrDefaultAsync(r => r.Id == id);
            if (restaurant == null)
                return ServiceResult<RestaurantResponse>.Fail(StatusCodes.Status404NotFound, "Restaurant not found.");

            // Only patch supplied fields
            if (request.Name != null) restaurant.Name = request.Name.Trim();
            if (request.City != null) restaurant.City = request.City.Trim();
            if (request.Address != null) restaurant.Address = request.Address.Trim();
            if (request.Rating.HasValue) restaurant.Rating = request.Rating.Value;
            if (request.IsActive.HasValue) restaurant.IsActive = request.IsActive.Value;

            await _db.SaveChangesAsync();

            await PublicReadCache.InvalidateActiveRestaurantsAsync(_cache);
            await PublicReadCache.InvalidateRestaurantMenusAsync(_cache, id);

            return ServiceResult<RestaurantResponse>.Ok(Map(restaurant), "Restaurant updated.");
        }

        private static RestaurantResponse Map(Restaurant r) => new()
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
