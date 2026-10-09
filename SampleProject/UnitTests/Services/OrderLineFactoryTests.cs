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
    public class OrderLineFactoryTests
    {
        private readonly Product _laptop = OrderTestData.Product("Laptop", 1399m);
        private readonly Product _mouse = OrderTestData.Product("Mouse", 25.5m);
        private readonly Mock<IProductRepository> _productRepository = new Mock<IProductRepository>();

        public OrderLineFactoryTests()
        {
            _productRepository.Setup(r => r.Get(_laptop.Id)).Returns(_laptop);
            _productRepository.Setup(r => r.Get(_mouse.Id)).Returns(_mouse);
        }

        [Fact]
        public void Create_PricesEachLineAtTheProductsCurrentNameAndPrice()
        {
            var lines = Factory().Create(new[] { Line(_laptop, 1), Line(_mouse, 2) }, NoLines);

            Assert.Equal(new[] { "Laptop x1 @1399", "Mouse x2 @25.5" }, lines.Select(l => $"{l.ProductName} x{l.Quantity} @{l.UnitPrice}"));
        }

        [Fact]
        public void Create_SameProductRequestedTwice_BecomesOneLine()
        {
            var line = Assert.Single(Factory().Create(new[] { Line(_mouse, 1), null, Line(_mouse, 2) }, NoLines));

            Assert.Equal(3, line.Quantity);
        }

        [Fact]
        public void Create_UnknownProduct_ThrowsValidationErrorForLines()
        {
            var unknown = Guid.NewGuid();

            var exception = Assert.Throws<DomainValidationException>(() => Factory().Create(new[] { new OrderLineDetails { ProductId = unknown, Quantity = 1 } }, NoLines));

            Assert.Equal("lines", exception.MemberName);
            Assert.Contains(unknown.ToString(), exception.Message);
        }

        [Theory]
        [InlineData(0, 2)]
        [InlineData(OrderLine.MaxQuantity, 1)]
        [InlineData(int.MaxValue, 1)]
        public void Create_QuantityOutOfRange_ThrowsValidationErrorAlsoForRepeatedProducts(int first, int second)
        {
            var exception = Assert.Throws<DomainValidationException>(() => Factory().Create(new[] { Line(_mouse, first), Line(_mouse, second) }, NoLines));

            Assert.Equal("quantity", exception.MemberName);
        }

        [Fact]
        public void Create_ProductAlreadyOnTheOrder_KeepsItsAgreedNameAndPriceWithoutALookup()
        {
            var current = new[] { new OrderLine(_mouse, 1) };
            _mouse.SetPrice(99m);
            _productRepository.Setup(r => r.Get(_mouse.Id)).Returns((Product)null);

            var line = Assert.Single(Factory().Create(new[] { Line(_mouse, 4) }, current));

            Assert.Equal(new object[] { "Mouse", 25.5m, 4 }, new object[] { line.ProductName, line.UnitPrice, line.Quantity });
        }

        private static readonly OrderLine[] NoLines = new OrderLine[0];

        private OrderLineFactory Factory()
        {
            return new OrderLineFactory(_productRepository.Object);
        }

        private static OrderLineDetails Line(Product product, int quantity)
        {
            return new OrderLineDetails { ProductId = product.Id, Quantity = quantity };
        }
    }
}