using System;
using System.Collections.Generic;

namespace Core.Services.Orders
{
    public class CreateOrderDetails
    {
        public Guid CustomerId { get; set; }
        public IEnumerable<OrderLineDetails> Lines { get; set; }
    }
}