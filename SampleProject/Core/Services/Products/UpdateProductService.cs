using System;
using BusinessEntities;
using Common;

namespace Core.Services.Products
{
    [AutoRegister(AutoRegisterTypes.Singleton)]
    public class UpdateProductService : IUpdateProductService
    {
        public void Update(Product product, ProductDetails details)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }
            if (details == null)
            {
                throw new ArgumentNullException(nameof(details));
            }

            product.SetName(details.Name);
            product.SetDescription(details.Description);
            product.SetPrice(details.Price);
            product.SetCategory(details.Category);
        }
    }
}