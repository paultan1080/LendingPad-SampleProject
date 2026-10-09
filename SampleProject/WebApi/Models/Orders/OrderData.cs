using System;
using System.Collections.Generic;
using System.Linq;
using BusinessEntities;

namespace WebApi.Models.Orders
{
    public class OrderData : IdObjectData
    {
        public OrderData(Order order) : base(order)
        {
            CustomerId = order.CustomerId;
            PlacedOn = order.PlacedOn;
            Status = new EnumData(order.Status);
            Lines = order.Lines.Select(line => new OrderLineData(line)).ToList();
            Total = order.Total;
        }

        public Guid CustomerId { get; set; }
        public DateTime PlacedOn { get; set; }
        public EnumData Status { get; set; }
        public IEnumerable<OrderLineData> Lines { get; set; }
        public decimal Total { get; set; }
    }
}