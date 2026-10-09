using BusinessEntities;
using Common.Exceptions;
using Core.Services.Orders;
using Data.Repositories;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class DeleteOrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepository = new Mock<IOrderRepository>();

        [Fact]
        public void Delete_PlacedOrder_IsDeleted()
        {
            var order = OrderTestData.PlacedOrder(new OrderLine(OrderTestData.Product("Mouse", 25.5m), 1));

            new DeleteOrderService(_orderRepository.Object).Delete(order);

            _orderRepository.Verify(r => r.Delete(order), Times.Once);
        }

        [Fact]
        public void Delete_PaidOrder_ThrowsConflictWithoutDeleting()
        {
            var order = OrderTestData.PlacedOrder(new OrderLine(OrderTestData.Product("Mouse", 25.5m), 1));
            order.ChangeStatus(OrderStatuses.Paid);

            Assert.Throws<ConflictException>(() => new DeleteOrderService(_orderRepository.Object).Delete(order));
            _orderRepository.Verify(r => r.Delete(It.IsAny<Order>()), Times.Never);
        }
    }
}