using System;
using BusinessEntities;
using Common;
using Common.Exceptions;
using Core.Factories;
using Data.Repositories;

namespace Core.Services.Orders
{
    [AutoRegister]
    public class CreateOrderService : ICreateOrderService
    {
        private readonly IClock _clock;
        private readonly IIdObjectFactory<Order> _orderFactory;
        private readonly IOrderLineFactory _orderLineFactory;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;

        public CreateOrderService(IIdObjectFactory<Order> orderFactory, IOrderRepository orderRepository, IUserRepository userRepository, IOrderLineFactory orderLineFactory, IClock clock)
        {
            _orderFactory = orderFactory;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _orderLineFactory = orderLineFactory;
            _clock = clock;
        }

        public Order Create(Guid id, CreateOrderDetails details)
        {
            if (details == null)
            {
                throw new ArgumentNullException(nameof(details));
            }

            EntityIds.EnsureAvailable(_orderRepository, id);
            var customer = FindCustomer(details.CustomerId);
            var lines = _orderLineFactory.Create(details.Lines, new OrderLine[0]);

            var order = _orderFactory.Create(id);
            order.Place(customer.Id, lines, _clock.UtcNow);
            _orderRepository.Save(order);
            return order;
        }

        private User FindCustomer(Guid customerId)
        {
            var customer = _userRepository.Get(customerId);
            if (customer == null)
            {
                throw new DomainValidationException("customerId", $"Customer '{customerId}' does not exist.");
            }
            if (customer.Type != UserTypes.Customer)
            {
                throw new DomainValidationException("customerId", $"Only customers can place orders; user '{customerId}' is {customer.Type}.");
            }
            return customer;
        }
    }
}