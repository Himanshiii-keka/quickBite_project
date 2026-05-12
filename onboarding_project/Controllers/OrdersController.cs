using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using startup_project.Common;
using startup_project.Models;
using startup_project.Models.Enums;
using startup_project.Services;

namespace startup_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        // ---------- User ----------

        /// <summary>Place an order from the current user's cart. Empties the cart on success.</summary>
        [HttpPost("checkout")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.GetUserId();
            var result = await _orderService.CheckoutAsync(userId);

            if (!result.Success || result.Data is null)
                return StatusCode(result.StatusCode, new ErrorMessageResponse { Message = result.Message });

            return StatusCode(result.StatusCode, result.Data);
        }

        /// <summary>Get the current user's order history (newest first).</summary>
        [HttpGet("my")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.GetUserId();
            var data = await _orderService.GetForUserAsync(userId);
            return Ok(data);
        }

        /// <summary>Get a specific order belonging to the current user (including its current status).</summary>
        [HttpGet("my/{id:int}")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyOrder(int id)
        {
            var userId = User.GetUserId();
            var result = await _orderService.GetByIdForUserAsync(userId, id);

            if (!result.Success || result.Data is null)
                return StatusCode(result.StatusCode, new ErrorMessageResponse { Message = result.Message });

            return Ok(result.Data);
        }

        // ---------- Admin ----------

        /// <summary>
        /// List ALL orders across the system. Optional filters by status and restaurant.
        /// </summary>
        /// <param name="status">Optional. 1=Placed, 2=Confirmed, 3=Preparing, 4=OutForDelivery, 5=Delivered, 6=Cancelled.</param>
        /// <param name="restaurantId">Optional. Restrict to a single restaurant.</param>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(List<OrderResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] OrderStatus? status, [FromQuery] int? restaurantId)
        {
            var data = await _orderService.GetAllAsync(status, restaurantId);
            return Ok(data);
        }

        /// <summary>Admin updates the status of an order. Delivered / Cancelled orders are terminal and can't be changed.</summary>
        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _orderService.UpdateStatusAsync(id, request.Status);

            if (!result.Success || result.Data is null)
                return StatusCode(result.StatusCode, new ErrorMessageResponse { Message = result.Message });

            return Ok(result.Data);
        }
    }
}
