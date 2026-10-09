using System;
using BusinessEntities;
using Core.Services.Products;
using Xunit;

namespace UnitTests.Services
{
    public class UpdateProductServiceTests
    {
        private readonly UpdateProductService _service = new UpdateProductService();

        [Fact]
        public void Update_CopiesEveryDetail()
        {
            var product = new Product();

            _service.Update(product, new ProductDetails { Name = "Support plan", Description = "Next business day", Price = 199m, Category = ProductCategories.Service });

            Assert.Equal("Support plan", product.Name);
            Assert.Equal("Next business day", product.Description);
            Assert.Equal(199m, product.Price);
            Assert.Equal(ProductCategories.Service, product.Category);
        }

        [Fact]
        public void Update_NullArguments_ThrowArgumentNullException()
        {
            Assert.Equal("product", Assert.Throws<ArgumentNullException>(() => _service.Update(null, new ProductDetails())).ParamName);
            Assert.Equal("details", Assert.Throws<ArgumentNullException>(() => _service.Update(new Product(), null)).ParamName);
        }
    }
}