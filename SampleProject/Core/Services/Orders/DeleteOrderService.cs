using BusinessEntities;
using Common;
using Data.Repositories;

namespace Core.Services.Orders
{
    [AutoRegister]
    public class DeleteOrderService : IDeleteOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public DeleteOrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        /// <summary>Only placed or cancelled orders can be deleted (409 otherwise).</summary>
        public void Delete(Order order)
        {
            order.EnsureCanBeDeleted();
            _orderRepository.Delete(order);
        }
    }
}