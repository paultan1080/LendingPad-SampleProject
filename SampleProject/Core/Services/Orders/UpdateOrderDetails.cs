using System.Collections.Generic;
using BusinessEntities;

namespace Core.Services.Orders
{
    public class UpdateOrderDetails
    {
        public OrderStatuses Status { get; set; }
        public IEnumerable<OrderLineDetails> Lines { get; set; }
    }
}