using Microsoft.Extensions.Caching.Distributed;

namespace startup_project.Common
{
    /// <summary>
    /// Keys and invalidation helpers for public read caches (Redis or in-memory <see cref="IDistributedCache"/>).
    /// Logical keys; StackExchange Redis prepends <c>InstanceName</c> from <c>Program.cs</c>.
    ///
    /// Consistency strategy: DB is always the source of truth. After every write, the affected
    /// cache key is invalidated so the next read repopulates from DB. Short TTLs (2 min for public
    /// data, 60 s for per-user cart) bound the staleness window in case invalidation fails.
    /// All callers must wrap cache operations in try-catch — Redis being unavailable must
    /// never cause an API failure (graceful degradation to DB).
    /// </summary>
    public static class PublicReadCache
    {
        // --- Public / shared keys ---
        public const string ActiveRestaurantsKey = "v1:restaurants:active";

        public static string MenuKey(int restaurantId, bool availableOnly) =>
            $"v1:menu:{restaurantId}:{(availableOnly ? "1" : "0")}";

        // --- Per-user cart key ---
        // Cart data lives in DB (required for checkout consistency). The key here is a
        // short-lived read cache for GET /cart so every page-load doesn't hit SQL Server.
        public static string CartKey(int userId) => $"v1:cart:{userId}";

        // --- Invalidation helpers ---
        public static Task InvalidateActiveRestaurantsAsync(IDistributedCache cache) =>
            cache.RemoveAsync(ActiveRestaurantsKey);

        /// <summary>Clears both user (available-only) and admin (full) menu caches for one restaurant.</summary>
        public static Task InvalidateRestaurantMenusAsync(IDistributedCache cache, int restaurantId) =>
            Task.WhenAll(
                cache.RemoveAsync(MenuKey(restaurantId, availableOnly: true)),
                cache.RemoveAsync(MenuKey(restaurantId, availableOnly: false)));

        public static Task InvalidateCartAsync(IDistributedCache cache, int userId) =>
            cache.RemoveAsync(CartKey(userId));
    }
}
