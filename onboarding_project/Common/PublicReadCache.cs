using Microsoft.Extensions.Caching.Distributed;

namespace startup_project.Common
{
    /// <summary>
    /// Keys and invalidation helpers for public read caches (Redis or in-memory <see cref="IDistributedCache"/>).
    /// Logical keys; StackExchange Redis prepends <c>InstanceName</c> from <c>Program.cs</c>.
    /// </summary>
    public static class PublicReadCache
    {
        public const string ActiveRestaurantsKey = "v1:restaurants:active";

        public static string MenuKey(int restaurantId, bool availableOnly) =>
            $"v1:menu:{restaurantId}:{(availableOnly ? "1" : "0")}";

        public static Task InvalidateActiveRestaurantsAsync(IDistributedCache cache) =>
            cache.RemoveAsync(ActiveRestaurantsKey);

        /// <summary>Clears both user (available-only) and admin (full) menu caches for one restaurant.</summary>
        public static Task InvalidateRestaurantMenusAsync(IDistributedCache cache, int restaurantId) =>
            Task.WhenAll(
                cache.RemoveAsync(MenuKey(restaurantId, availableOnly: true)),
                cache.RemoveAsync(MenuKey(restaurantId, availableOnly: false)));
    }
}
