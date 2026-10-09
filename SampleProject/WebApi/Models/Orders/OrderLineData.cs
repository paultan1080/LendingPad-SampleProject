using System;
using BusinessEntities;

namespace WebApi.Models.Orders
{
    public class OrderLineData
    {
        public OrderLineData(OrderLine line)
        {
            ProductId = line.ProductId;
            ProductName = line.ProductName;
            UnitPrice = line.UnitPrice;
            Quantity = line.Quantity;
            Total = line.Total;
        }

        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }
}