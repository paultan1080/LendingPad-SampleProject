using System;
using System.Linq;
using BusinessEntities;
using Common.Exceptions;
using Xunit;

namespace UnitTests.Entities
{
    public class OrderTests
    {
        private static readonly Guid CustomerId = Guid.NewGuid();
        private static readonly DateTime PlacedOn = new DateTime(2026, 10, 8, 9, 30, 0, DateTimeKind.Utc);

        private readonly Product _laptop = NewProduct("Laptop", 1399m);
        private readonly Product _mouse = NewProduct("Mouse", 25.5m);

        [Fact]
        public void OrderLine_KeepsTheProductsNameAndPriceAtPricingTime()
        {
            var line = new OrderLine(_mouse, 2);
            _mouse.SetName("Renamed mouse");
            _mouse.SetPrice(99m);

            Assert.Equal(_mouse.Id, line.ProductId);
            Assert.Equal("Mouse", line.ProductName);
            Assert.Equal(25.5m, line.UnitPrice);
            Assert.Equal(51m, line.Total);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(OrderLine.MaxQuantity + 1)]
        public void OrderLine_QuantityOutOfRange_ThrowsValidationErrorForQuantity(int quantity)
        {
            Assert.Equal("quantity", Assert.Throws<DomainValidationException>(() => new OrderLine(_mouse, quantity)).MemberName);
            Assert.Equal("quantity", Assert.Throws<DomainValidationException>(() => new OrderLine(_mouse, 1).WithQuantity(quantity)).MemberName);
        }

        [Fact]
        public void OrderLine_WithQuantity_KeepsTheAgreedNameAndPrice()
        {
            var line = new OrderLine(_mouse, 2);
            _mouse.SetName("Renamed mouse");
            _mouse.SetPrice(99m);

            var changed = line.WithQuantity(5);

            Assert.Equal(new object[] { _mouse.Id, "Mouse", 25.5m, 5, 127.5m }, new object[] { changed.ProductId, changed.ProductName, changed.UnitPrice, changed.Quantity, changed.Total });
            Assert.Equal(2, line.Quantity);
        }

        [Fact]
        public void Place_RecordsCustomerLinesAndTime_AndStartsPlaced()
        {
            var order = PlacedOrder();

            Assert.Equal(CustomerId, order.CustomerId);
            Assert.Equal(PlacedOn, order.PlacedOn);
            Assert.Equal(OrderStatuses.Placed, order.Status);
            Assert.Equal(new[] { "Laptop", "Mouse" }, order.Lines.Select(line => line.ProductName));
            Assert.Equal(1399m + 51m, order.Total);
        }

        [Fact]
        public void Place_WithoutCustomer_ThrowsValidationErrorForCustomer()
        {
            var exception = Assert.Throws<DomainValidationException>(() => new Order().Place(Guid.Empty, new[] { new OrderLine(_mouse, 1) }, PlacedOn));

            Assert.Equal("customerId", exception.MemberName);
        }

        [Fact]
        public void Place_WithoutLines_ThrowsValidationErrorForLines()
        {
            Assert.Equal("lines", Assert.Throws<DomainValidationException>(() => new Order().Place(CustomerId, new OrderLine[0], PlacedOn)).MemberName);
            Assert.Equal("lines", Assert.Throws<DomainValidationException>(() => new Order().Place(CustomerId, null, PlacedOn)).MemberName);
        }

        [Fact]
        public void Place_SameProductOnTwoLines_ThrowsValidationErrorForLines()
        {
            var exception = Assert.Throws<DomainValidationException>(() => new Order().Place(CustomerId, new[] { new OrderLine(_mouse, 1), new OrderLine(_mouse, 2) }, PlacedOn));

            Assert.Equal("lines", exception.MemberName);
        }

        [Fact]
        public void Place_Twice_Throws()
        {
            var order = PlacedOrder();

            Assert.Throws<InvalidOperationException>(() => order.Place(CustomerId, new[] { new OrderLine(_mouse, 1) }, PlacedOn));
        }

        [Fact]
        public void Lines_CannotBeModifiedFromOutside()
        {
            Assert.False(PlacedOrder().Lines is System.Collections.Generic.List<OrderLine>);
        }

        [Fact]
        public void ReplaceLines_WhilePlaced_ReplacesThemAndTheTotal()
        {
            var order = PlacedOrder();

            order.ReplaceLines(new[] { new OrderLine(_mouse, 3) });

            Assert.Equal(76.5m, order.Total);
        }

        [Theory]
        [InlineData(OrderStatuses.Paid)]
        [InlineData(OrderStatuses.Cancelled)]
        public void ReplaceLines_AfterPlacement_ThrowsConflict(OrderStatuses status)
        {
            var order = PlacedOrder();
            order.ChangeStatus(status);

            Assert.Throws<ConflictException>(() => order.ReplaceLines(new[] { new OrderLine(_mouse, 3) }));
            Assert.Equal(2, order.Lines.Count());
        }

        [Theory]
        [InlineData(new[] { OrderStatuses.Paid })]
        [InlineData(new[] { OrderStatuses.Cancelled })]
        [InlineData(new[] { OrderStatuses.Paid, OrderStatuses.Shipped, OrderStatuses.Delivered })]
        [InlineData(new[] { OrderStatuses.Paid, OrderStatuses.Cancelled })]
        public void ChangeStatus_AlongTheLifecycle_Succeeds(OrderStatuses[] path)
        {
            var order = PlacedOrder();

            foreach (var status in path)
            {
                order.ChangeStatus(status);
            }

            Assert.Equal(path.Last(), order.Status);
        }

        [Theory]
        [InlineData(new[] { OrderStatuses.Shipped })]
        [InlineData(new[] { OrderStatuses.Delivered })]
        [InlineData(new[] { OrderStatuses.Paid, OrderStatuses.Placed })]
        [InlineData(new[] { OrderStatuses.Paid, OrderStatuses.Shipped, OrderStatuses.Cancelled })]
        [InlineData(new[] { OrderStatuses.Cancelled, OrderStatuses.Paid })]
        public void ChangeStatus_AgainstTheLifecycle_ThrowsConflictAndKeepsTheStatus(OrderStatuses[] path)
        {
            var order = PlacedOrder();
            foreach (var status in path.Take(path.Length - 1))
            {
                order.ChangeStatus(status);
            }
            var before = order.Status;

            Assert.Throws<ConflictException>(() => order.ChangeStatus(path.Last()));
            Assert.Equal(before, order.Status);
        }

        [Fact]
        public void ChangeStatus_ToTheCurrentStatus_IsANoOp()
        {
            var order = PlacedOrder();

            order.ChangeStatus(OrderStatuses.Placed);

            Assert.Equal(OrderStatuses.Placed, order.Status);
        }

        [Fact]
        public void ChangeStatus_UndefinedValue_ThrowsValidationErrorForStatus()
        {
            var exception = Assert.Throws<DomainValidationException>(() => PlacedOrder().ChangeStatus((OrderStatuses)42));

            Assert.Equal("status", exception.MemberName);
        }

        [Theory]
        [InlineData(new OrderStatuses[0], true)]
        [InlineData(new[] { OrderStatuses.Cancelled }, true)]
        [InlineData(new[] { OrderStatuses.Paid }, false)]
        [InlineData(new[] { OrderStatuses.Paid, OrderStatuses.Shipped }, false)]
        [InlineData(new[] { OrderStatuses.Paid, OrderStatuses.Shipped, OrderStatuses.Delivered }, false)]
        public void EnsureCanBeDeleted_OnlyForPlacedOrCancelledOrders(OrderStatuses[] path, bool deletable)
        {
            var order = PlacedOrder();
            foreach (var status in path)
            {
                order.ChangeStatus(status);
            }

            Assert.Equal(deletable, order.CanBeDeleted);
            if (deletable)
            {
                order.EnsureCanBeDeleted();
            }
            else
            {
                Assert.Throws<ConflictException>(() => order.EnsureCanBeDeleted());
            }
        }

        private Order PlacedOrder()
        {
            var order = new Order();
            order.Place(CustomerId, new[] { new OrderLine(_laptop, 1), new OrderLine(_mouse, 2) }, PlacedOn);
            return order;
        }

        private static Product NewProduct(string name, decimal price)
        {
            var product = new Product();
            product.SetName(name);
            product.SetPrice(price);
            return product;
        }
    }
}