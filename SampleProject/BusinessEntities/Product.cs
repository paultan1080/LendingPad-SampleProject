using System;
using Common.Exceptions;

namespace BusinessEntities
{
    /// <summary>Something the company sells, at a list price.</summary>
    public class Product : IdObject
    {
        public const decimal MaxPrice = 1000000000m;

        private ProductCategories _category = ProductCategories.Hardware;
        private string _description;
        private string _name;
        private decimal _price;

        public string Name
        {
            get => _name;
            private set => _name = value;
        }

        public string Description
        {
            get => _description;
            private set => _description = value;
        }

        public decimal Price
        {
            get => _price;
            private set => _price = value;
        }

        public ProductCategories Category
        {
            get => _category;
            private set => _category = value;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainValidationException(nameof(name), "Name is required.");
            }
            _name = name;
        }

        /// <summary>Optional; null or blank removes the description.</summary>
        public void SetDescription(string description)
        {
            _description = string.IsNullOrWhiteSpace(description) ? null : description;
        }

        public void SetPrice(decimal price)
        {
            if (price < 0 || price > MaxPrice)
            {
                throw new DomainValidationException(nameof(price), $"Price must be between 0 and {MaxPrice}.");
            }
            if (decimal.Round(price, 2) != price)
            {
                throw new DomainValidationException(nameof(price), "Price cannot have more than 2 decimal places.");
            }
            _price = price;
        }

        public void SetCategory(ProductCategories category)
        {
            if (!Enum.IsDefined(typeof(ProductCategories), category))
            {
                throw new DomainValidationException(nameof(category), $"'{category}' is not a valid product category.");
            }
            _category = category;
        }
    }
}