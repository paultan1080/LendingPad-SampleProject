using System.ComponentModel.DataAnnotations;
using System.Linq;
using BusinessEntities;
using Core.Services.Orders;

namespace WebApi.Models.Orders
{
    /// <summary>The customer and placement time of an order is immutable; lines and status can change.</summary>
    public class UpdateOrderModel
    {
        [Required]
        [EnumDataType(typeof(OrderStatuses))]
        public OrderStatuses? Status { get; set; }

        [Required]
        [MinLength(1)]
        public OrderLineModel[] Lines { get; set; }

        /// <summary>
        /// Maps the request to the update use case. Call only on a validated model
        /// (ValidateModelAttribute runs globally), since required values are unwrapped.
        /// </summary>
        public UpdateOrderDetails ToUpdateOrderDetails()
        {
            return new UpdateOrderDetails
                   {
                       Status = Status.Value,
                       Lines = Lines.Where(line => line != null).Select(line => line.ToOrderLineDetails()).ToList()
                   };
        }
    }
}