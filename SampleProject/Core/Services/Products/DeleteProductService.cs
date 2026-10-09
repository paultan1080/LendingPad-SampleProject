using BusinessEntities;
using Common;
using Data.Repositories;

namespace Core.Services.Products
{
    /// <summary>
    /// Deleting a product does not affect existing orders.
    /// </summary>
    [AutoRegister]
    public class DeleteProductService : IDeleteProductService
    {
        private readonly IProductRepository _productRepository;

        public DeleteProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public void Delete(Product product)
        {
            _productRepository.Delete(product);
        }
    }
}