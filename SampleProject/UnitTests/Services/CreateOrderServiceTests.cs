using System;
using System.Collections.Generic;
using System.Linq;
using BusinessEntities;
using Common.Exceptions;
using Core.Factories;
using Core.Services;
using Core.Services.Orders;
using Data.Repositories;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class CreateOrderServiceTests
    {
        private static readonly Guid Id = Guid.NewGuid();

        private readonly User _customer = OrderTestData.User(UserTypes.Customer);
        private readonly OrderLine[] _lines = { new OrderLine(OrderTestData.Product("Mouse", 25.5m), 2) };
        private readonly Mock<IOrderLineFactory> _orderLineFactory = new Mock<IOrderLineFactory>();
        private readonly Mock<IOrderRepository> _orderRepository = new Mock<IOrderRepository>();
        private readonly Mock<IUserRepository> _userRepository = new Mock<IUserRepository>();
        private readonly Mock<IClock> _clock = new Mock<IClock>();
        private readonly CreateOrderDetails _details;

        public CreateOrderServiceTests()
        {
            _details = new CreateOrderDetails { CustomerId = _customer.Id, Lines = new[] { new OrderLineDetails { ProductId = Guid.NewGuid(), Quantity = 2 } } };
            _userRepository.Setup(r => r.Get(_customer.Id)).Returns(_customer);
            _orderLineFactory.Setup(f => f.Create(_details.Lines, It.Is<IEnumerable<OrderLine>>(current => !current.Any()))).Returns(_lines);
            _clock.Setup(c => c.UtcNow).Returns(OrderTestData.Now);
        }

        [Fact]
        public void Create_PlacesAndStoresTheOrderForTheCustomerNow()
        {
            var order = CreateService().Create(Id, _details);

            Assert.Equal(Id, order.Id);
            Assert.Equal(_customer.Id, order.CustomerId);
            Assert.Equal(OrderTestData.Now, order.PlacedOn);
            Assert.Equal(OrderStatuses.Placed, order.Status);
            Assert.Equal(_lines, order.Lines);
            _orderRepository.Verify(r => r.Save(order), Times.Once);
        }

        [Fact]
        public void Create_UnknownCustomer_ThrowsValidationErrorForCustomer()
        {
            _details.CustomerId = Guid.NewGuid();

            var exception = Assert.Throws<DomainValidationException>(() => CreateService().Create(Id, _details));

            Assert.Equal("customerId", exception.MemberName);
            _orderRepository.Verify(r => r.Save(It.IsAny<Order>()), Times.Never);
        }

        [Theory]
        [InlineData(UserTypes.Admin)]
        [InlineData(UserTypes.Employee)]
        public void Create_UserWhoIsNotACustomer_ThrowsValidationErrorForCustomer(UserTypes type)
        {
            var user = OrderTestData.User(type);
            _userRepository.Setup(r => r.Get(user.Id)).Returns(user);
            _details.CustomerId = user.Id;

            var exception = Assert.Throws<DomainValidationException>(() => CreateService().Create(Id, _details));

            Assert.Equal("customerId", exception.MemberName);
            Assert.Contains("Only customers", exception.Message);
        }

        [Fact]
        public void Create_IdAlreadyTaken_ThrowsConflict()
        {
            _orderRepository.Setup(r => r.Get(Id)).Returns(new Order());

            var exception = Assert.Throws<ConflictException>(() => CreateService().Create(Id, _details));

            Assert.Equal($"An order with id '{Id}' already exists.", exception.Message);
        }

        private CreateOrderService CreateService()
        {
            return new CreateOrderService(new IdObjectFactory<Order>(), _orderRepository.Object, _userRepository.Object, _orderLineFactory.Object, _clock.Object);
        }
    }
}