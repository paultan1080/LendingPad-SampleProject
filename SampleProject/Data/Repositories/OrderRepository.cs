using System.Collections.Generic;
using System.Linq;
using BusinessEntities;
using Common;

namespace Data.Repositories
{
    [AutoRegister]
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(IDataSession session) : base(session)
        {
        }

        public IEnumerable<Order> Get(OrderFilter filter, int skip, int take)
        {
            // Newest first.
            return Matching(filter).OrderByDescending(order => order.PlacedOn)
                                   .ThenBy(order => order.Id)
                                   .Skip(skip)
                                   .Take(take)
                                   .ToList();
        }

        public bool Any(OrderFilter filter)
        {
            return Matching(filter).Any();
        }

        private IEnumerable<Order> Matching(OrderFilter filter)
        {
            IEnumerable<Order> orders = Session.Query<Order>();
            if (filter.CustomerId != null)
            {
                orders = orders.Where(order => order.CustomerId == filter.CustomerId);
            }
            if (filter.Status != null)
            {
                orders = orders.Where(order => order.Status == filter.Status);
            }
            if (filter.ProductId != null)
            {
                orders = orders.Where(order => order.Lines.Any(line => line.ProductId == filter.ProductId));
            }
            if (filter.PlacedFrom != null)
            {
                orders = orders.Where(order => order.PlacedOn >= filter.PlacedFrom);
            }
            if (filter.PlacedBefore != null)
            {
                orders = orders.Where(order => order.PlacedOn < filter.PlacedBefore);
            }
            return orders;
        }
    }
}