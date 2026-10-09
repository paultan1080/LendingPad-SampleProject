using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Core.Services.Orders;

namespace WebApi.Models.Orders
{
    public class CreateOrderModel
    {
        [Required]
        public Guid? CustomerId { get; set; }

        [Required]
        [MinLength(1)]
        public OrderLineModel[] Lines { get; set; }

        /// <summary>
        /// Maps the request to the create use case. Call only on a validated model
        /// (ValidateModelAttribute runs globally), since required values are unwrapped.
        /// </summary>
        public CreateOrderDetails ToCreateOrderDetails()
        {
            return new CreateOrderDetails
                   {
                       CustomerId = CustomerId.Value,
                       Lines = Lines.Where(line => line != null).Select(line => line.ToOrderLineDetails()).ToList()
                   };
        }
    }
}