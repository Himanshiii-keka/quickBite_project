using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using startup_project.Common;
using startup_project.Models;
using startup_project.Models.Requests;
using startup_project.Models.ViewModels;
using startup_project.Services;

namespace startup_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>Get the current user's cart. Returns an empty cart if none exists yet.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(CartViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.GetUserId();
            var cart = await _cartService.GetCartAsync(userId);
            return Ok(cart);
        }

        /// <summary>
        /// Add a menu item to the cart, or increment quantity if it's already there.
        /// A cart can only contain items from one restaurant at a time.
        /// </summary>
        [HttpPost("items")]
        [ProducesResponseType(typeof(CartViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            var result = await _cartService.AddItemAsync(userId, request);

            if (!result.Success || result.Data is null)
                return StatusCode(result.StatusCode, new ErrorMessageResponse { Message = result.Message });

            return Ok(result.Data);
        }

        /// <summary>Remove a single menu item from the cart.</summary>
        [HttpDelete("items/{menuItemId:int}")]
        [ProducesResponseType(typeof(CartViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveItem(int menuItemId)
        {
            var userId = User.GetUserId();
            var result = await _cartService.RemoveItemAsync(userId, menuItemId);

            if (!result.Success || result.Data is null)
                return StatusCode(result.StatusCode, new ErrorMessageResponse { Message = result.Message });

            return Ok(result.Data);
        }

        /// <summary>Empty the cart entirely (also frees it to accept a different restaurant next).</summary>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Clear()
        {
            var userId = User.GetUserId();
            var result = await _cartService.ClearAsync(userId);
            return Ok(new { message = result.Message });
        }
    }
}
