using System.Collections.Generic;
using BusinessEntities;

namespace Data.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        IEnumerable<Order> Get(OrderFilter filter, int skip, int take);

        bool Any(OrderFilter filter);
    }
}