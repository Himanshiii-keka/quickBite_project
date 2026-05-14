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

        public OrderService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ---------- Checkout (User) ----------

        /// <summary>
        /// Creates an Order from the user's current cart, snapshots prices into OrderItems,
        /// then empties the cart. All wrapped in a transaction.
        /// </summary>
        public async Task<ServiceResult<OrderViewModel>> CheckoutAsync(int userId)
        {
            var cart = await _db.Carts
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    Cart = c,
                    Restaurant = c.Restaurant,
                    CartItems = c.CartItems
                })
                .FirstOrDefaultAsync();

            if (cart == null || cart.CartItems.Count == 0 || cart.Cart.RestaurantId == null)
                return ServiceResult<OrderViewModel>.Fail(StatusCodes.Status400BadRequest, "Your cart is empty.");

            if (cart.Restaurant == null || !cart.Restaurant.IsActive)
                return ServiceResult<OrderViewModel>.Fail(StatusCodes.Status400BadRequest, "This restaurant is no longer accepting orders.");

            // Reject if any item became unavailable between adding to cart and checkout
            var unavailable = cart.CartItems.FirstOrDefault(ci => !ci.MenuItem.IsAvailable);
            if (unavailable != null)
                return ServiceResult<OrderViewModel>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Item '{unavailable.MenuItem.Name}' is no longer available. Please remove it before checking out.");

            await using var tx = await _db.Database.BeginTransactionAsync();

            var order = new Order
            {
                UserId = userId,
                RestaurantId = cart.Cart.RestaurantId.Value,
                Status = OrderStatus.Placed,
                OrderPlacedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    MenuItemId = ci.MenuItemId,
                    ItemName = ci.MenuItem.Name,
                    ItemPrice = ci.MenuItem.Price,
                    Quantity = ci.Quantity,
                    LineTotal = ci.MenuItem.Price * ci.Quantity
                }).ToList()
            };

            order.TotalAmount = order.OrderItems.Sum(oi => oi.LineTotal);

            _db.Orders.Add(order);

            // Empty the cart now that the order is captured
            _db.CartItems.RemoveRange(cart.CartItems);
            cart.Cart.RestaurantId = null;
            cart.Cart.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            var orderResponse = new OrderViewModel
            {
                Id = order.Id,
                RestaurantId = order.RestaurantId,
                RestaurantName = cart.Restaurant.Name,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                OrderPlacedAt = order.OrderPlacedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ItemName = oi.ItemName,
                    ItemPrice = oi.ItemPrice,
                    Quantity = oi.Quantity,
                    LineTotal = oi.LineTotal
                }).ToList()
            };

            return ServiceResult<OrderViewModel>.Created(orderResponse, "Order placed successfully.");
        }

        // ---------- View Orders (User) ----------

        public async Task<List<OrderViewModel>> GetForUserAsync(int userId)
        {
            return await _db.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderPlacedAt)
                .Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    RestaurantId = o.RestaurantId,
                    RestaurantName = o.Restaurant.Name,        // From Restaurant table
                    Status = o.Status.ToString(),
                    TotalAmount = o.TotalAmount,
                    OrderPlacedAt = o.OrderPlacedAt,
                    UpdatedAt = o.UpdatedAt,
                    CustomerName = o.User.Name,                // From User table
                    CustomerEmail = o.User.Email,              // From User table
                    Items = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        ItemName = oi.ItemName,
                        ItemPrice = oi.ItemPrice,
                        Quantity = oi.Quantity,
                        LineTotal = oi.LineTotal
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<ServiceResult<OrderViewModel>> GetByIdForUserAsync(int userId, int orderId)
        {
            var order = await _db.Orders
                .AsNoTracking()
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return ServiceResult<OrderViewModel>.Fail(StatusCodes.Status404NotFound, "Order not found.");

            if (order.UserId != userId)
                return ServiceResult<OrderViewModel>.Fail(StatusCodes.Status403Forbidden, "You do not have access to this order.");

            return ServiceResult<OrderViewModel>.Ok(MapToResponse(order, order.Restaurant, includeCustomer: false));
        }

        // ---------- View Orders (Admin) ----------

        public async Task<List<OrderViewModel>> GetAllAsync(OrderStatus? statusFilter, int? restaurantFilter)
        {
            var query = _db.Orders.AsNoTracking().AsQueryable();

            if (statusFilter.HasValue)
                query = query.Where(o => o.Status == statusFilter.Value);

            if (restaurantFilter.HasValue)
                query = query.Where(o => o.RestaurantId == restaurantFilter.Value);

            return await query
                .OrderByDescending(o => o.OrderPlacedAt)
                .Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    RestaurantId = o.RestaurantId,
                    RestaurantName = o.Restaurant.Name,
                    Status = o.Status.ToString(),
                    TotalAmount = o.TotalAmount,
                    OrderPlacedAt = o.OrderPlacedAt,
                    UpdatedAt = o.UpdatedAt,
                    CustomerName = o.User.Name,
                    CustomerEmail = o.User.Email,
                    Items = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        ItemName = oi.ItemName,
                        ItemPrice = oi.ItemPrice,
                        Quantity = oi.Quantity,
                        LineTotal = oi.LineTotal
                    }).ToList()
                })
                .ToListAsync();
        }

        // ---------- Update Status (Admin) ----------

        public async Task<ServiceResult<OrderViewModel>> UpdateStatusAsync(int orderId, OrderStatus newStatus)
        {
            // Defend against arbitrary int casts on the wire
            if (!Enum.IsDefined(typeof(OrderStatus), newStatus))
                return ServiceResult<OrderViewModel>.Fail(StatusCodes.Status400BadRequest, "Invalid order status value.");

            var order = await _db.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return ServiceResult<OrderViewModel>.Fail(StatusCodes.Status404NotFound, "Order not found.");

            // Terminal states can't transition further
            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
                return ServiceResult<OrderViewModel>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Order is already {order.Status} and cannot be changed.");

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var response = MapToResponse(order, order.Restaurant, includeCustomer: true);
            response.CustomerName = order.User.Name;
            response.CustomerEmail = order.User.Email;

            return ServiceResult<OrderViewModel>.Ok(response, "Order status updated.");
        }

        // ---------- Helpers ----------

        private static OrderViewModel MapToResponse(Order order, Restaurant restaurant, bool includeCustomer)
        {
            return new OrderViewModel
            {
                Id = order.Id,
                RestaurantId = order.RestaurantId,
                RestaurantName = restaurant.Name,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                OrderPlacedAt = order.OrderPlacedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ItemName = oi.ItemName,
                    ItemPrice = oi.ItemPrice,
                    Quantity = oi.Quantity,
                    LineTotal = oi.LineTotal
                }).ToList(),
                CustomerName = includeCustomer ? order.User?.Name : null,
                CustomerEmail = includeCustomer ? order.User?.Email : null
            };
        }
    }
}
