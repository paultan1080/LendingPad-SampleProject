using BusinessEntities;

namespace Core.Services.Products
{
    public interface IDeleteProductService
    {
        void Delete(Product product);
    }
}