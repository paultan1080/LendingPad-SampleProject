using System;
using System.Collections.Generic;
using System.Linq;
using BusinessEntities;
using Common;

namespace Core.Services.Orders
{
    [AutoRegister]
    public class UpdateOrderService : IUpdateOrderService
    {
        private readonly IOrderLineFactory _orderLineFactory;

        public UpdateOrderService(IOrderLineFactory orderLineFactory)
        {
            _orderLineFactory = orderLineFactory;
        }

        /// <summary>
        /// Replaces the lines if the requested items differ (only allowed while the order is Placed),
        /// then moves the order to the requested status. Products already on the order keep their
        /// agreed price; only added products are priced now. Resending the current items does nothing.
        /// </summary>
        public void Update(Order order, UpdateOrderDetails details)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }
            if (details == null)
            {
                throw new ArgumentNullException(nameof(details));
            }

            var requested = OrderLineDetails.Combine(details.Lines);
            if (!HasItems(order, requested))
            {
                order.ReplaceLines(_orderLineFactory.Create(requested, order.Lines));
            }
            order.ChangeStatus(details.Status);
        }

        private static bool HasItems(Order order, IReadOnlyList<OrderLineDetails> requested)
        {
            var current = order.Lines.ToDictionary(line => line.ProductId, line => line.Quantity);
            return requested.Count == current.Count &&
                   requested.All(line => current.TryGetValue(line.ProductId, out var quantity) && quantity == line.Quantity);
        }
    }
}