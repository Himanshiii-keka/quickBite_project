using System.ComponentModel.DataAnnotations;
using startup_project.Models.Enums;

namespace startup_project.Models.Requests
{
    /// <summary>
    /// Request model for updating the status of an order (admin action).
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        /// <summary>1=Placed, 2=Confirmed, 3=Preparing, 4=OutForDelivery, 5=Delivered, 6=Cancelled</summary>
        /// <example>2</example>
        [Required]
        public OrderStatus Status { get; set; }
    }
}
