using BusinessEntities;
using Common.Exceptions;
using Core.Services.Users;
using Data.Repositories;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class DeleteUserServiceTests
    {
        private readonly User _customer = OrderTestData.User(UserTypes.Customer);
        private readonly Mock<IOrderRepository> _orderRepository = new Mock<IOrderRepository>();
        private readonly Mock<IUserRepository> _userRepository = new Mock<IUserRepository>();

        [Fact]
        public void Delete_UserWithoutOrders_IsDeleted()
        {
            Service().Delete(_customer);

            _userRepository.Verify(r => r.Delete(_customer), Times.Once);
        }

        [Fact]
        public void Delete_UserWithOrders_ThrowsConflictWithoutDeleting()
        {
            _orderRepository.Setup(r => r.Any(It.Is<OrderFilter>(f => f.CustomerId == _customer.Id))).Returns(true);

            var exception = Assert.Throws<ConflictException>(() => Service().Delete(_customer));

            Assert.Contains("has orders", exception.Message);
            _userRepository.Verify(r => r.Delete(It.IsAny<User>()), Times.Never);
        }

        private DeleteUserService Service()
        {
            return new DeleteUserService(_userRepository.Object, _orderRepository.Object);
        }
    }
}