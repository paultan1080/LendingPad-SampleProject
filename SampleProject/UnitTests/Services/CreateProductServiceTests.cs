using System;
using BusinessEntities;
using Common.Exceptions;
using Core.Factories;
using Core.Services.Products;
using Data.Repositories;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class CreateProductServiceTests
    {
        private static readonly Guid Id = Guid.NewGuid();

        private readonly ProductDetails _details = new ProductDetails { Name = "Laptop", Price = 1499.99m, Category = ProductCategories.Hardware };
        private readonly Mock<IIdObjectFactory<Product>> _productFactory = new Mock<IIdObjectFactory<Product>>();
        private readonly Mock<IProductRepository> _productRepository = new Mock<IProductRepository>();
        private readonly Mock<IUpdateProductService> _updateProductService = new Mock<IUpdateProductService>();

        private CreateProductService CreateService()
        {
            return new CreateProductService(_productFactory.Object, _productRepository.Object, _updateProductService.Object);
        }

        [Fact]
        public void Create_NewId_PopulatesAndStoresTheProduct()
        {
            var product = new Product();
            _productFactory.Setup(f => f.Create(Id)).Returns(product);

            var result = CreateService().Create(Id, _details);

            Assert.Same(product, result);
            _updateProductService.Verify(u => u.Update(product, _details), Times.Once);
            _productRepository.Verify(r => r.Save(product), Times.Once);
        }

        [Fact]
        public void Create_IdAlreadyTaken_ThrowsConflict()
        {
            _productRepository.Setup(r => r.Get(Id)).Returns(new Product());

            var exception = Assert.Throws<ConflictException>(() => CreateService().Create(Id, _details));

            Assert.Equal($"A product with id '{Id}' already exists.", exception.Message);
            _productRepository.Verify(r => r.Save(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public void Create_EmptyId_ThrowsValidationErrorForId()
        {
            var exception = Assert.Throws<DomainValidationException>(() => CreateService().Create(Guid.Empty, _details));

            Assert.Equal("id", exception.MemberName);
            Assert.Equal("A non-empty product id is required.", exception.Message);
        }
    }
}