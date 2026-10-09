using System;
using System.Collections.Generic;
using System.Linq;
using BusinessEntities;
using Common.Exceptions;

namespace Core.Services.Orders
{
    /// <summary>A requested order line: which product, how many.</summary>
    public class OrderLineDetails
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }

        /// <summary>
        /// Validates requested lines and merges repeated products into one line each, in first-seen order.
        /// </summary>
        public static IReadOnlyList<OrderLineDetails> Combine(IEnumerable<OrderLineDetails> lines)
        {
            var requested = (lines ?? Enumerable.Empty<OrderLineDetails>()).Where(line => line != null).ToList();
            var combined = requested.GroupBy(line => line.ProductId)
                                    .Select(group => new { ProductId = group.Key, Quantity = group.Sum(line => (long)line.Quantity) })
                                    .ToList();
            if (requested.Any(line => line.Quantity < 1) || combined.Any(line => line.Quantity > OrderLine.MaxQuantity))
            {
                throw new DomainValidationException("quantity", $"Quantity must be between 1 and {OrderLine.MaxQuantity}.");
            }

            return combined.Select(line => new OrderLineDetails { ProductId = line.ProductId, Quantity = (int)line.Quantity }).ToList();
        }
    }
}