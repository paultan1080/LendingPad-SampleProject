using System;
using System.Collections.Generic;
using BusinessEntities;
using Common;
using Common.Exceptions;
using Data.Repositories;

namespace Core.Services.Orders
{
    [AutoRegister]
    public class GetOrderService : IGetOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public Order GetOrder(Guid id)
        {
            return _orderRepository.Get(id);
        }

        public IEnumerable<Order> GetOrders(OrderFilter filter, int skip, int take)
        {
            Paging.EnsureValid(skip, take);
            filter = filter ?? new OrderFilter();
            if (filter.Status != null && !Enum.IsDefined(typeof(OrderStatuses), filter.Status.Value))
            {
                throw new DomainValidationException("status", $"'{filter.Status}' is not a valid order status.");
            }
            var normalized = new OrderFilter
                             {
                                 CustomerId = filter.CustomerId,
                                 Status = filter.Status,
                                 ProductId = filter.ProductId,
                                 PlacedFrom = AsUtc(filter.PlacedFrom),
                                 PlacedBefore = AsUtc(filter.PlacedBefore)
                             };
            if (normalized.PlacedFrom >= normalized.PlacedBefore)
            {
                throw new DomainValidationException("placedFrom", "PlacedFrom must be earlier than PlacedBefore.");
            }

            return _orderRepository.Get(normalized, skip, take);
        }

        // Orders are timestamped in UTC; no timezone defaults to UTC.
        private static DateTime? AsUtc(DateTime? value)
        {
            if (value == null)
            {
                return null;
            }
            return value.Value.Kind == DateTimeKind.Local
                ? value.Value.ToUniversalTime()
                : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
        }
    }
}