using System;
using System.Linq;
using BusinessEntities;
using Common.Exceptions;
using Core.Services.Orders;
using Data.Repositories;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class UpdateOrderServiceTests
    {
        private readonly Product _laptop = OrderTestData.Product("Laptop", 1399m);
        private readonly Product _mouse = OrderTestData.Product("Mouse", 25.5m);
        private readonly Product _keyboard = OrderTestData.Product("Keyboard", 50m);
        private readonly Mock<IProductRepository> _productRepository = new Mock<IProductRepository>();
        private readonly Order _order;

        public UpdateOrderServiceTests()
        {
            _order = OrderTestData.PlacedOrder(new OrderLine(_laptop, 1), new OrderLine(_mouse, 2));
            foreach (var product in new[] { _laptop, _mouse, _keyboard })
            {
                _productRepository.Setup(r => r.Get(product.Id)).Returns(product);
            }
        }

        [Fact]
        public void Update_SameItemsInAnyOrder_OnlyChangesTheStatus()
        {
            Service().Update(_order, Details(OrderStatuses.Paid, Line(_mouse, 1), Line(_laptop, 1), Line(_mouse, 1)));

            Assert.Equal(OrderStatuses.Paid, _order.Status);
            Assert.Equal(1399m + 51m, _order.Total);
            _productRepository.Verify(r => r.Get(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public void Update_ChangedQuantity_KeepsTheAgreedPriceOfEveryLine()
        {
            _laptop.SetPrice(1999m);
            _mouse.SetPrice(99m);

            Service().Update(_order, Details(OrderStatuses.Placed, Line(_laptop, 2), Line(_mouse, 2)));

            Assert.Equal(new[] { "Laptop x2 @1399", "Mouse x2 @25.5" }, Describe(_order));
        }

        [Fact]
        public void Update_AddedProduct_IsPricedNowAndRemovedProductDisappears()
        {
            _keyboard.SetPrice(45m);

            Service().Update(_order, Details(OrderStatuses.Placed, Line(_laptop, 1), Line(_keyboard, 1)));

            Assert.Equal(new[] { "Laptop x1 @1399", "Keyboard x1 @45" }, Describe(_order));
        }

        [Fact]
        public void Update_ProductOnTheOrderWasDeleted_OtherLinesCanStillChange()
        {
            _productRepository.Setup(r => r.Get(_mouse.Id)).Returns((Product)null);

            Service().Update(_order, Details(OrderStatuses.Placed, Line(_laptop, 3), Line(_mouse, 2)));

            Assert.Equal(new[] { "Laptop x3 @1399", "Mouse x2 @25.5" }, Describe(_order));
        }

        [Fact]
        public void Update_DifferentItemsAfterPayment_ThrowsConflict()
        {
            _order.ChangeStatus(OrderStatuses.Paid);

            Assert.Throws<ConflictException>(() => Service().Update(_order, Details(OrderStatuses.Paid, Line(_mouse, 3))));
            Assert.Equal(2, _order.Lines.Count());
        }

        [Fact]
        public void Update_TransitionAgainstTheLifecycle_ThrowsConflict()
        {
            Assert.Throws<ConflictException>(() => Service().Update(_order, Details(OrderStatuses.Delivered, Line(_laptop, 1), Line(_mouse, 2))));
            Assert.Equal(OrderStatuses.Placed, _order.Status);
        }

        [Fact]
        public void Update_NullArguments_ThrowArgumentNullException()
        {
            Assert.Equal("order", Assert.Throws<ArgumentNullException>(() => Service().Update(null, new UpdateOrderDetails())).ParamName);
            Assert.Equal("details", Assert.Throws<ArgumentNullException>(() => Service().Update(_order, null)).ParamName);
        }

        private UpdateOrderService Service()
        {
            return new UpdateOrderService(new OrderLineFactory(_productRepository.Object));
        }

        private static string[] Describe(Order order)
        {
            return order.Lines.Select(l => $"{l.ProductName} x{l.Quantity} @{l.UnitPrice}").ToArray();
        }

        private static UpdateOrderDetails Details(OrderStatuses status, params OrderLineDetails[] lines)
        {
            return new UpdateOrderDetails { Status = status, Lines = lines };
        }

        private static OrderLineDetails Line(Product product, int quantity)
        {
            return new OrderLineDetails { ProductId = product.Id, Quantity = quantity };
        }
    }
}