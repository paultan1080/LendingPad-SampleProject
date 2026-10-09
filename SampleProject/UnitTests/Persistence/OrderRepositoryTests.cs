using System;
using System.Collections.Generic;
using System.Linq;
using BusinessEntities;
using Core.Factories;
using Data.InMemory;
using Data.Repositories;
using Xunit;

namespace UnitTests.Persistence
{
    public class OrderRepositoryTests
    {
        private static readonly Guid Alice = Guid.NewGuid();
        private static readonly Guid Bob = Guid.NewGuid();
        private static readonly DateTime Day = new DateTime(2026, 10, 8, 0, 0, 0, DateTimeKind.Utc);

        private readonly Dictionary<Guid, string> _labels = new Dictionary<Guid, string>();
        private readonly InMemoryStore _store = new InMemoryStore();
        private readonly Product _laptop = NewProduct("Laptop");
        private readonly Product _mouse = NewProduct("Mouse");

        public OrderRepositoryTests()
        {
            var session = new InMemorySession(_store);
            var repository = new OrderRepository(session);
            repository.Save(NewOrder("A1", Alice, Day.AddHours(9), OrderStatuses.Placed, new OrderLine(_laptop, 1)));
            repository.Save(NewOrder("A2", Alice, Day.AddHours(12), OrderStatuses.Paid, new OrderLine(_mouse, 2)));
            repository.Save(NewOrder("B1", Bob, Day.AddDays(1), OrderStatuses.Paid, new OrderLine(_laptop, 1), new OrderLine(_mouse, 1)));
            session.Commit();
        }

        [Fact]
        public void Get_NoCriteria_ReturnsNewestFirst()
        {
            Assert.Equal(new[] { "B1", "A2", "A1" }, Labels(new OrderFilter()));
        }

        [Fact]
        public void Get_ByCustomerStatusAndProduct()
        {
            Assert.Equal(new[] { "A2", "A1" }, Labels(new OrderFilter { CustomerId = Alice }));
            Assert.Equal(new[] { "B1", "A2" }, Labels(new OrderFilter { Status = OrderStatuses.Paid }));
            Assert.Equal(new[] { "B1", "A1" }, Labels(new OrderFilter { ProductId = _laptop.Id }));
            Assert.Equal(new[] { "A2" }, Labels(new OrderFilter { CustomerId = Alice, ProductId = _mouse.Id }));
        }

        [Fact]
        public void Get_ByPlacementWindow_FromInclusiveBeforeExclusive()
        {
            Assert.Equal(new[] { "A2", "A1" }, Labels(new OrderFilter { PlacedFrom = Day.AddHours(9), PlacedBefore = Day.AddDays(1) }));
            Assert.Equal(new[] { "B1" }, Labels(new OrderFilter { PlacedFrom = Day.AddDays(1) }));
        }

        [Fact]
        public void Any_TellsWhetherAnOrderMatches()
        {
            var repository = new OrderRepository(new InMemorySession(_store));

            Assert.True(repository.Any(new OrderFilter { CustomerId = Bob }));
            Assert.False(repository.Any(new OrderFilter { CustomerId = Guid.NewGuid() }));
        }

        [Fact]
        public void Get_Pages()
        {
            Assert.Equal(new[] { "A2" }, new OrderRepository(new InMemorySession(_store)).Get(new OrderFilter(), 1, 1).Select(Label));
        }

        private string[] Labels(OrderFilter filter)
        {
            return new OrderRepository(new InMemorySession(_store)).Get(filter, 0, 50).Select(Label).ToArray();
        }

        private string Label(Order order)
        {
            return _labels[order.Id];
        }

        private Order NewOrder(string label, Guid customerId, DateTime placedOn, OrderStatuses status, params OrderLine[] lines)
        {
            var order = new IdObjectFactory<Order>().Create(Guid.NewGuid());
            order.Place(customerId, lines, placedOn);
            order.ChangeStatus(status);
            _labels.Add(order.Id, label);
            return order;
        }

        private static Product NewProduct(string name)
        {
            var product = new IdObjectFactory<Product>().Create(Guid.NewGuid());
            product.SetName(name);
            product.SetPrice(10m);
            return product;
        }
    }
}