using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using startup_project.Models;
using startup_project.Models.ViewModels;
using startup_project.Services;

namespace startup_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RestaurantsController : ControllerBase
    {
        private readonly RestaurantService _restaurantService;
        private readonly MenuItemService _menuItemService;

        public RestaurantsController(RestaurantService restaurantService, MenuItemService menuItemService)
        {
            _restaurantService = restaurantService;
            _menuItemService = menuItemService;
        }

        // ---------- Browse (User) ----------

        /// <summary>List all ACTIVE restaurants. Visible to any logged-in user.</summary>
        [HttpGet]
        [Authorize(Roles = "User")]
        [ProducesResponseType(typeof(List<RestaurantViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActive()
        {
            var data = await _restaurantService.GetActiveAsync();
            return Ok(data);
        }

        /// <summary>Get details of a single active restaurant.</summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(typeof(RestaurantViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _restaurantService.GetByIdAsync(id, activeOnly: true);
            if (!result.Success || result.Data is null)
                return StatusCode(result.StatusCode, new ErrorMessageResponse { Message = result.Message });

            return Ok(result.Data);
        }

        /// <summary>List the available menu items for an active restaurant.</summary>
        [HttpGet("{id:int}/menu")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(typeof(List<MenuItemViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorMessageResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMenu(int id)
        {
            var result = await _menuItemService.GetByRestaurantAsync(id, availableOnly: true);
            if (!result.Success || result.Data is null)
                return StatusCode(result.StatusCode, new ErrorMessageResponse { Message = result.Message });

            return Ok(result.Data);
        }
    }
}
