using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using startup_project.Models;
using startup_project.Services;

namespace startup_project.Controllers
{
    /// <summary>
    /// Admin-only endpoints to manage restaurants and menu items.
    /// All routes require the Admin role.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly RestaurantService _restaurantService;
        private readonly MenuItemService _menuItemService;

        public AdminController(RestaurantService restaurantService, MenuItemService menuItemService)
        {
            _restaurantService = restaurantService;
            _menuItemService = menuItemService;
        }

        // ---------- Restaurants ----------

        /// <summary>List ALL restaurants — active and inactive — for admin management.</summary>
        [HttpGet("restaurants")]
        [ProducesResponseType(typeof(List<RestaurantResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllRestaurants()
        {
            var data = await _restaurantService.GetAllAsync();
            return Ok(data);
        }

        /// <summary>Create a new restaurant.</summary>
        [HttpPost("restaurants")]
        [ProducesResponseType(typeof(RestaurantResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRestaurant([FromBody] CreateRestaurantRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _restaurantService.CreateAsync(request);
            return StatusCode(result.StatusCode, result.Data);
        }

        /// <summary>Update one or more fields of an existing restaurant. Only supplied fields are changed.</summary>
        [HttpPut("restaurants/{id:int}")]
        [ProducesResponseType(typeof(RestaurantResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRestaurant(int id, [FromBody] UpdateRestaurantRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _restaurantService.UpdateAsync(id, request);

            if (!result.Success || result.Data is null)
                return StatusCode(result.StatusCode, new ErrorMessageResponse { Message = result.Message });

            return Ok(result.Data);
        }

        // ---------- Menu Items ----------

        /// <summary>List ALL menu items for a restaurant (including unavailable ones).</summary>
        [HttpGet("restaurants/{restaurantId:int}/menu")]
        [ProducesResponseType(typeof(List<MenuItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMenuForAdmin(int restaurantId)
        {
            var result = await _menuItemService.GetByRestaurantAsync(restaurantId, availableOnly: false);
            if (!result.Success || result.Data is null)
                return StatusCode(result.StatusCode, new ErrorMessageResponse { Message = result.Message });

            return Ok(result.Data);
        }

        /// <summary>Add a menu item to a restaurant.</summary>
        [HttpPost("restaurants/{restaurantId:int}/menu")]
        [ProducesResponseType(typeof(MenuItemResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateMenuItem(int restaurantId, [FromBody] CreateMenuItemRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _menuItemService.CreateAsync(restaurantId, request);

            if (!result.Success || result.Data is null)
                return StatusCode(result.StatusCode, new ErrorMessageResponse { Message = result.Message });

            return StatusCode(result.StatusCode, result.Data);
        }

        /// <summary>Update fields of a menu item, e.g. price or availability. Only supplied fields are changed.</summary>
        [HttpPut("menu/{id:int}")]
        [ProducesResponseType(typeof(MenuItemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMenuItem(int id, [FromBody] UpdateMenuItemRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _menuItemService.UpdateAsync(id, request);

            if (!result.Success || result.Data is null)
                return StatusCode(result.StatusCode, new ErrorMessageResponse { Message = result.Message });

            return Ok(result.Data);
        }

        /// <summary>
        /// Delete a menu item. Rejected with 409 if the item appears in any historical order —
        /// in that case set IsAvailable=false via the update endpoint instead.
        /// </summary>
        [HttpDelete("menu/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var result = await _menuItemService.DeleteAsync(id);

            if (!result.Success)
                return StatusCode(result.StatusCode, new ErrorMessageResponse { Message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}
