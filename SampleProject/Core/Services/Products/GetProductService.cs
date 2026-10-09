using System;
using System.Collections.Generic;
using BusinessEntities;
using Common;
using Common.Exceptions;
using Data.Repositories;

namespace Core.Services.Products
{
    [AutoRegister]
    public class GetProductService : IGetProductService
    {
        private readonly IProductRepository _productRepository;

        public GetProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public Product GetProduct(Guid id)
        {
            return _productRepository.Get(id);
        }

        public IEnumerable<Product> GetProducts(ProductFilter filter, int skip, int take)
        {
            Paging.EnsureValid(skip, take);
            filter = filter ?? new ProductFilter();
            if (filter.Category != null && !Enum.IsDefined(typeof(ProductCategories), filter.Category.Value))
            {
                throw new DomainValidationException("category", $"'{filter.Category}' is not a valid product category.");
            }
            if (filter.MinPrice > filter.MaxPrice)
            {
                throw new DomainValidationException("minPrice", "MinPrice cannot be greater than MaxPrice.");
            }

            return _productRepository.Get(filter, skip, take);
        }
    }
}