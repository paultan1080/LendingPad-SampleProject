using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BusinessEntities;
using Core.Services;
using Core.Services.Products;
using Moq;
using WebApi.Controllers;
using WebApi.Models.Products;
using Xunit;

namespace WebApi.Tests.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<ICreateProductService> _createProductService = new Mock<ICreateProductService>();
        private readonly Mock<IDeleteProductService> _deleteProductService = new Mock<IDeleteProductService>();
        private readonly Mock<IGetProductService> _getProductService = new Mock<IGetProductService>();
        private readonly Mock<IUpdateProductService> _updateProductService = new Mock<IUpdateProductService>();

        [Fact]
        public void CreateProduct_PassesTheRequestAsProductDetails()
        {
            var id = Guid.NewGuid();
            var model = new ProductModel { Name = "Laptop", Description = "15-inch", Price = 1499.99m, Category = ProductCategories.Hardware };
            _createProductService.Setup(s => s.Create(id, It.IsAny<ProductDetails>())).Returns(new Product());

            var response = CreateController().CreateProduct(id, model);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _createProductService.Verify(s => s.Create(id, It.Is<ProductDetails>(d =>
                d.Name == "Laptop" && d.Description == "15-inch" && d.Price == 1499.99m && d.Category == ProductCategories.Hardware)), Times.Once);
        }

        [Fact]
        public void UnknownProduct_GetUpdateAndDelete_Return404WithoutSideEffects()
        {
            var controller = CreateController();
            var id = Guid.NewGuid();

            Assert.Equal(HttpStatusCode.NotFound, controller.GetProduct(id).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, controller.UpdateProduct(id, new ProductModel()).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, controller.DeleteProduct(id).StatusCode);
            _updateProductService.Verify(s => s.Update(It.IsAny<Product>(), It.IsAny<ProductDetails>()), Times.Never);
            _deleteProductService.Verify(s => s.Delete(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public void GetProducts_BuildsTheFilterAndUsesTheDefaultPage()
        {
            var laptop = new Product();
            laptop.SetName("Laptop");
            _getProductService.Setup(s => s.GetProducts(It.IsAny<ProductFilter>(), 0, Paging.DefaultPageSize)).Returns(new[] { laptop });

            var response = CreateController().GetProducts(name: "lap", category: ProductCategories.Hardware, minPrice: 1m, maxPrice: 2000m);

            var product = Assert.Single((IEnumerable<ProductData>)((ObjectContent)response.Content).Value);
            Assert.Equal("Laptop", product.Name);
            _getProductService.Verify(s => s.GetProducts(It.Is<ProductFilter>(f =>
                f.Name == "lap" && f.Category == ProductCategories.Hardware && f.MinPrice == 1m && f.MaxPrice == 2000m), 0, Paging.DefaultPageSize));
        }

        private ProductController CreateController()
        {
            var request = new HttpRequestMessage();
            request.SetConfiguration(new HttpConfiguration());
            return new ProductController(_createProductService.Object, _deleteProductService.Object, _getProductService.Object, _updateProductService.Object)
                   {
                       Request = request
                   };
        }
    }
}