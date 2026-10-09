using System;
using System.Collections.Generic;
using System.Linq;
using BusinessEntities;
using Common;

namespace Data.Repositories
{
    [AutoRegister]
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(IDataSession session) : base(session)
        {
        }

        public IEnumerable<Product> Get(ProductFilter filter, int skip, int take)
        {
            IEnumerable<Product> products = Session.Query<Product>();
            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var name = filter.Name.Trim();
                products = products.Where(product => product.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (filter.Category != null)
            {
                products = products.Where(product => product.Category == filter.Category);
            }
            if (filter.MinPrice != null)
            {
                products = products.Where(product => product.Price >= filter.MinPrice);
            }
            if (filter.MaxPrice != null)
            {
                products = products.Where(product => product.Price <= filter.MaxPrice);
            }

            return products.OrderBy(product => product.Name, StringComparer.OrdinalIgnoreCase)
                           .ThenBy(product => product.Id)
                           .Skip(skip)
                           .Take(take)
                           .ToList();
        }
    }
}