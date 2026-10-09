using BusinessEntities;
using Common.Exceptions;
using Core.Services;
using Core.Services.Products;
using Data.Repositories;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class GetProductServiceTests
    {
        private readonly Mock<IProductRepository> _productRepository = new Mock<IProductRepository>();

        private GetProductService CreateService()
        {
            return new GetProductService(_productRepository.Object);
        }

        [Fact]
        public void GetProducts_PassesTheFilterAndPageToTheRepository()
        {
            var filter = new ProductFilter { Category = ProductCategories.Hardware, MaxPrice = 1000m };
            var products = new[] { new Product() };
            _productRepository.Setup(r => r.Get(filter, 10, 20)).Returns(products);

            Assert.Same(products, CreateService().GetProducts(filter, 10, 20));
        }

        [Fact]
        public void GetProducts_NoFilter_MeansEverything()
        {
            CreateService().GetProducts(null, 0, 10);

            _productRepository.Verify(r => r.Get(It.Is<ProductFilter>(f => f.Name == null && f.Category == null && f.MinPrice == null && f.MaxPrice == null), 0, 10));
        }

        [Fact]
        public void GetProducts_MinPriceAboveMaxPrice_ThrowsValidationError()
        {
            var exception = Assert.Throws<DomainValidationException>(() => CreateService().GetProducts(new ProductFilter { MinPrice = 10m, MaxPrice = 5m }, 0, 10));

            Assert.Equal("minPrice", exception.MemberName);
        }

        [Theory]
        [InlineData(42)]
        [InlineData(3 | 4)]
        public void GetProducts_UndefinedCategory_ThrowsValidationError(int category)
        {
            var exception = Assert.Throws<DomainValidationException>(() => CreateService().GetProducts(new ProductFilter { Category = (ProductCategories)category }, 0, 10));

            Assert.Equal("category", exception.MemberName);
        }

        [Theory]
        [InlineData(-1, 10, "skip")]
        [InlineData(0, 0, "take")]
        [InlineData(0, Paging.MaxPageSize + 1, "take")]
        public void GetProducts_InvalidPage_ThrowsValidationError(int skip, int take, string member)
        {
            var exception = Assert.Throws<DomainValidationException>(() => CreateService().GetProducts(new ProductFilter(), skip, take));

            Assert.Equal(member, exception.MemberName);
        }
    }
}