using System.Collections.Generic;
using System.Linq;
using BusinessEntities;
using Common;
using Common.Exceptions;
using Data.Repositories;

namespace Core.Services.Orders
{
    [AutoRegister]
    public class OrderLineFactory : IOrderLineFactory
    {
        private readonly IProductRepository _productRepository;

        public OrderLineFactory(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public IReadOnlyList<OrderLine> Create(IEnumerable<OrderLineDetails> lines, IEnumerable<OrderLine> currentLines)
        {
            var current = (currentLines ?? Enumerable.Empty<OrderLine>()).ToDictionary(line => line.ProductId);
            return OrderLineDetails.Combine(lines)
                                   .Select(line => current.TryGetValue(line.ProductId, out var existing)
                                       ? existing.WithQuantity(line.Quantity)
                                       : new OrderLine(FindProduct(line), line.Quantity))
                                   .ToList();
        }

        private Product FindProduct(OrderLineDetails line)
        {
            var product = _productRepository.Get(line.ProductId);
            if (product == null)
            {
                throw new DomainValidationException("lines", $"Product '{line.ProductId}' does not exist.");
            }
            return product;
        }
    }
}