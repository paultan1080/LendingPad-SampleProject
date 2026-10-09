using BusinessEntities;
using Common;
using Common.Exceptions;
using Data.Repositories;

namespace Core.Services.Users
{
    [AutoRegister]
    public class DeleteUserService : IDeleteUserService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;

        public DeleteUserService(IUserRepository userRepository, IOrderRepository orderRepository)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
        }

        /// <summary>A user with orders cannot be deleted (409).</summary>
        public void Delete(User user)
        {
            if (_orderRepository.Any(new OrderFilter { CustomerId = user.Id }))
            {
                throw new ConflictException($"User '{user.Id}' has orders and cannot be deleted.");
            }
            _userRepository.Delete(user);
        }

        /// <summary>Resets the users for test data; orders are not touched.</summary>
        public void DeleteAll()
        {
            _userRepository.DeleteAll();
        }
    }
}