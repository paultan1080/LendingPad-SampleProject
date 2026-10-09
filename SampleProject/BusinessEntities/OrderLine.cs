using System;
using Common.Exceptions;

namespace BusinessEntities
{
    /// <summary>
    /// A product on an order, at the name and unit price the product had when the line was priced.
    /// Later catalog changes (or deleting the product) do not alter the order.
    /// </summary>
    public class OrderLine
    {
        public const int MaxQuantity = 10000;

        private Guid _productId;
        private string _productName;
        private int _quantity;
        private decimal _unitPrice;

        public OrderLine(Product product, int quantity)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            _productId = product.Id;
            _productName = product.Name;
            _unitPrice = product.Price;
            _quantity = ValidQuantity(quantity);
        }

        // For rehydration from storage.
        private OrderLine()
        {
        }

        public Guid ProductId
        {
            get => _productId;
            private set => _productId = value;
        }

        public string ProductName
        {
            get => _productName;
            private set => _productName = value;
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            private set => _unitPrice = value;
        }

        public int Quantity
        {
            get => _quantity;
            private set => _quantity = value;
        }

        public decimal Total => _unitPrice * _quantity;

        /// <summary>The same product at the same agreed name and price, in another quantity.</summary>
        public OrderLine WithQuantity(int quantity)
        {
            return new OrderLine
                   {
                       _productId = _productId,
                       _productName = _productName,
                       _unitPrice = _unitPrice,
                       _quantity = ValidQuantity(quantity)
                   };
        }

        private static int ValidQuantity(int quantity)
        {
            if (quantity < 1 || quantity > MaxQuantity)
            {
                throw new DomainValidationException(nameof(quantity), $"Quantity must be between 1 and {MaxQuantity}.");
            }
            return quantity;
        }
    }
}