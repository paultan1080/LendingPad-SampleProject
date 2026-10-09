using System;
using BusinessEntities;
using Common.Exceptions;
using Core.Services;
using Core.Services.Orders;
using Data.Repositories;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class GetOrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepository = new Mock<IOrderRepository>();

        [Fact]
        public void GetOrders_PassesTheCriteriaAndPageToTheRepository()
        {
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            Service().GetOrders(new OrderFilter { CustomerId = customerId, Status = OrderStatuses.Paid, ProductId = productId }, 5, 10);

            _orderRepository.Verify(r => r.Get(It.Is<OrderFilter>(f => f.CustomerId == customerId && f.Status == OrderStatuses.Paid && f.ProductId == productId), 5, 10));
        }

        [Fact]
        public void GetOrders_TimesWithoutAZone_AreTakenAsUtc()
        {
            var from = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Unspecified);

            Service().GetOrders(new OrderFilter { PlacedFrom = from }, 0, 10);

            _orderRepository.Verify(r => r.Get(It.Is<OrderFilter>(f => f.PlacedFrom.Value.Kind == DateTimeKind.Utc && f.PlacedFrom.Value.Ticks == from.Ticks), 0, 10));
        }

        [Fact]
        public void GetOrders_LocalTimes_AreConvertedToUtc()
        {
            var before = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Local);

            Service().GetOrders(new OrderFilter { PlacedBefore = before }, 0, 10);

            _orderRepository.Verify(r => r.Get(It.Is<OrderFilter>(f => f.PlacedBefore == before.ToUniversalTime() && f.PlacedBefore.Value.Kind == DateTimeKind.Utc), 0, 10));
        }

        [Fact]
        public void GetOrders_EmptyOrInvertedWindow_ThrowsValidationError()
        {
            var day = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

            var exception = Assert.Throws<DomainValidationException>(() => Service().GetOrders(new OrderFilter { PlacedFrom = day, PlacedBefore = day }, 0, 10));

            Assert.Equal("placedFrom", exception.MemberName);
        }

        [Theory]
        [InlineData(42)]
        [InlineData(0)]
        public void GetOrders_UndefinedStatus_ThrowsValidationError(int status)
        {
            Assert.Equal("status", Assert.Throws<DomainValidationException>(() => Service().GetOrders(new OrderFilter { Status = (OrderStatuses)status }, 0, 10)).MemberName);
        }

        [Fact]
        public void GetOrders_InvalidPage_ThrowsValidationError()
        {
            Assert.Equal("take", Assert.Throws<DomainValidationException>(() => Service().GetOrders(null, 0, Paging.MaxPageSize + 1)).MemberName);
        }

        private GetOrderService Service()
        {
            return new GetOrderService(_orderRepository.Object);
        }
    }
}