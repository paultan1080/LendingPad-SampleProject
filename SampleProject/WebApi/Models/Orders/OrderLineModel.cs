using System;
using System.ComponentModel.DataAnnotations;
using BusinessEntities;
using Core.Services.Orders;

namespace WebApi.Models.Orders
{
    public class OrderLineModel
    {
        [Required]
        public Guid? ProductId { get; set; }

        [Required]
        [Range(1, OrderLine.MaxQuantity)]
        public int? Quantity { get; set; }

        public OrderLineDetails ToOrderLineDetails()
        {
            return new OrderLineDetails { ProductId = ProductId.Value, Quantity = Quantity.Value };
        }
    }
}