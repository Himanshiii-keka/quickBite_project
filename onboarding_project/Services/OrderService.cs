using Microsoft.EntityFrameworkCore;
using startup_project.Data;
using startup_project.Models;
using startup_project.Models.Common;
using startup_project.Models.Enums;
using startup_project.Models.ViewModels;

namespace startup_project.Services
{
    public class OrderService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext db, ILogger<OrderService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ---------- Checkout (User) ----------

        /// <summary>
        /// Creates an Order from the user's current cart, snapshots prices into OrderItems,
        /// then empties the cart. All wrapped in a transaction.
        /// Each table is queried separately — no SQL JOINs.
        /// </summary>
        public async Task<ServiceResult<UserOrderViewModel>> CheckoutAsync(int userId)
        {
            var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.RestaurantId.HasValue)
                return ServiceResult<UserOrderViewModel>.Fail(StatusCodes.Status400BadRequest, "Your cart is empty.");

            var cartItems = await _db.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            if (cartItems.Count == 0)
                return ServiceResult<UserOrderViewModel>.Fail(StatusCodes.Status400BadRequest, "Your cart is empty.");

            var restaurant = await _db.Restaurants
                .FirstOrDefaultAsync(r => r.Id == cart.RestaurantId.Value);

            if (restaurant == null || !restaurant.IsActive)
                return ServiceResult<UserOrderViewModel>.Fail(StatusCodes.Status400BadRequest, "This restaurant is no longer accepting orders.");

            var menuItemIds = cartItems.Select(ci => ci.MenuItemId).ToList();
            var menuItems = await _db.MenuItems
                .Where(m => menuItemIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id);

            var unavailable = cartItems.FirstOrDefault(ci =>
                menuItems.TryGetValue(ci.MenuItemId, out var mi) && !mi.IsAvailable);

            if (unavailable != null)
            {
                var name = menuItems[unavailable.MenuItemId].Name;
                return ServiceResult<UserOrderViewModel>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Item '{name}' is no longer available. Please remove it before checking out.");
            }

            await using var tx = await _db.Database.BeginTransactionAsync();

            var orderItemsList = cartItems.Select(ci =>
            {
                var mi = menuItems[ci.MenuItemId];
                return new OrderItem
                {
                    MenuItemId = ci.MenuItemId,
                    ItemName = mi.Name,
                    ItemPrice = mi.Price,
                    Quantity = ci.Quantity,
                    LineTotal = mi.Price * ci.Quantity
                };
            }).ToList();

            var order = new Order
            {
                UserId = userId,
                RestaurantId = cart.RestaurantId.Value,
                Status = OrderStatus.Placed,
                OrderPlacedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                TotalAmount = orderItemsList.Sum(oi => oi.LineTotal),
                OrderItems = orderItemsList
            };

            _db.Orders.Add(order);
            _db.CartItems.RemoveRange(cartItems);
            cart.RestaurantId = null;
            cart.UpdatedAtUtc = DateTime.UtcNow;

            try
            {
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error during checkout for user {UserId}.", userId);
                return ServiceResult<UserOrderViewModel>.Fail(StatusCodes.Status500InternalServerError,
                    "Failed to place order due to a database error.");
            }

            return ServiceResult<UserOrderViewModel>.Created(
                MapToUserView(order, restaurant.Name, orderItemsList),
                "Order placed successfully.");
        }

        // ---------- View Orders (User) ----------

        /// <summary>Returns the user's order history. Tables queried individually — no JOINs.</summary>
        public async Task<List<UserOrderViewModel>> GetForUserAsync(int userId)
        {
            var orders = await _db.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderPlacedAt)
                .ToListAsync();

            if (orders.Count == 0) return new List<UserOrderViewModel>();

            var orderIds = orders.Select(o => o.Id).ToList();
            var restaurantIds = orders.Select(o => o.RestaurantId).Distinct().ToList();

            var orderItems = await _db.OrderItems
                .AsNoTracking()
                .Where(oi => orderIds.Contains(oi.OrderId))
                .ToListAsync();

            var restaurantNames = await _db.Restaurants
                .AsNoTracking()
                .Where(r => restaurantIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.Name);

            var itemsByOrder = orderItems
                .GroupBy(oi => oi.OrderId)
                .ToDictionary(g => g.Key, g => g.ToList());

            return orders.Select(o => MapToUserView(
                o,
                restaurantNames.GetValueOrDefault(o.RestaurantId, string.Empty),
                itemsByOrder.GetValueOrDefault(o.Id, new List<OrderItem>())
            )).ToList();
        }

        public async Task<ServiceResult<UserOrderViewModel>> GetByIdForUserAsync(int userId, int orderId)
        {
            var order = await _db.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return ServiceResult<UserOrderViewModel>.Fail(StatusCodes.Status404NotFound, "Order not found.");

            if (order.UserId != userId)
                return ServiceResult<UserOrderViewModel>.Fail(StatusCodes.Status403Forbidden, "You do not have access to this order.");

            var orderItems = await _db.OrderItems
                .AsNoTracking()
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();

            var restaurantName = await _db.Restaurants
                .AsNoTracking()
                .Where(r => r.Id == order.RestaurantId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync() ?? string.Empty;

            return ServiceResult<UserOrderViewModel>.Ok(MapToUserView(order, restaurantName, orderItems));
        }

        // ---------- View Orders (Admin) ----------

        /// <summary>Returns all orders with customer info. Tables queried individually — no JOINs.</summary>
        public async Task<List<AdminOrderViewModel>> GetAllAsync(OrderStatus? statusFilter, int? restaurantFilter)
        {
            var query = _db.Orders.AsNoTracking().AsQueryable();

            if (statusFilter.HasValue)
                query = query.Where(o => o.Status == statusFilter.Value);

            if (restaurantFilter.HasValue)
                query = query.Where(o => o.RestaurantId == restaurantFilter.Value);

            var orders = await query
                .OrderByDescending(o => o.OrderPlacedAt)
                .ToListAsync();

            if (orders.Count == 0) return new List<AdminOrderViewModel>();

            var orderIds = orders.Select(o => o.Id).ToList();
            var restaurantIds = orders.Select(o => o.RestaurantId).Distinct().ToList();
            var userIds = orders.Select(o => o.UserId).Distinct().ToList();

            var orderItems = await _db.OrderItems
                .AsNoTracking()
                .Where(oi => orderIds.Contains(oi.OrderId))
                .ToListAsync();

            var restaurantNames = await _db.Restaurants
                .AsNoTracking()
                .Where(r => restaurantIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.Name);

            var users = await _db.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var itemsByOrder = orderItems
                .GroupBy(oi => oi.OrderId)
                .ToDictionary(g => g.Key, g => g.ToList());

            return orders.Select(o =>
            {
                var vm = MapToAdminView(
                    o,
                    restaurantNames.GetValueOrDefault(o.RestaurantId, string.Empty),
                    itemsByOrder.GetValueOrDefault(o.Id, new List<OrderItem>()));

                if (users.TryGetValue(o.UserId, out var user))
                {
                    vm.CustomerName = user.Name;
                    vm.CustomerEmail = user.Email;
                }

                return vm;
            }).ToList();
        }

        // ---------- Update Status (Admin) ----------

        public async Task<ServiceResult<AdminOrderViewModel>> UpdateStatusAsync(int orderId, OrderStatus newStatus)
        {
            if (!Enum.IsDefined(typeof(OrderStatus), newStatus))
                return ServiceResult<AdminOrderViewModel>.Fail(StatusCodes.Status400BadRequest, "Invalid order status value.");

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return ServiceResult<AdminOrderViewModel>.Fail(StatusCodes.Status404NotFound, "Order not found.");

            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
                return ServiceResult<AdminOrderViewModel>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Order is already {order.Status} and cannot be changed.");

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating status for order {OrderId}.", orderId);
                return ServiceResult<AdminOrderViewModel>.Fail(StatusCodes.Status500InternalServerError,
                    "Failed to update order status due to a database error.");
            }

            var orderItems = await _db.OrderItems
                .AsNoTracking()
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();

            var restaurantName = await _db.Restaurants
                .AsNoTracking()
                .Where(r => r.Id == order.RestaurantId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync() ?? string.Empty;

            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == order.UserId);

            var vm = MapToAdminView(order, restaurantName, orderItems);
            vm.CustomerName = user?.Name ?? string.Empty;
            vm.CustomerEmail = user?.Email ?? string.Empty;

            return ServiceResult<AdminOrderViewModel>.Ok(vm, "Order status updated.");
        }

        // ---------- Helpers ----------

        private static UserOrderViewModel MapToUserView(Order order, string restaurantName, List<OrderItem> items)
        {
            return new UserOrderViewModel
            {
                Id = order.Id,
                RestaurantId = order.RestaurantId,
                RestaurantName = restaurantName,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                OrderPlacedAt = order.OrderPlacedAt,
                UpdatedAt = order.UpdatedAt,
                Items = items.Select(oi => new OrderItemViewModel
                {
                    ItemName = oi.ItemName,
                    ItemPrice = oi.ItemPrice,
                    Quantity = oi.Quantity,
                    LineTotal = oi.LineTotal
                }).ToList()
            };
        }

        private static AdminOrderViewModel MapToAdminView(Order order, string restaurantName, List<OrderItem> items)
        {
            return new AdminOrderViewModel
            {
                Id = order.Id,
                RestaurantId = order.RestaurantId,
                RestaurantName = restaurantName,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                OrderPlacedAt = order.OrderPlacedAt,
                UpdatedAt = order.UpdatedAt,
                Items = items.Select(oi => new OrderItemViewModel
                {
                    ItemName = oi.ItemName,
                    ItemPrice = oi.ItemPrice,
                    Quantity = oi.Quantity,
                    LineTotal = oi.LineTotal
                }).ToList()
            };
        }
    }
}
