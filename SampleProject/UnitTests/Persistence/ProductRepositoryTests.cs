using System;
using System.Linq;
using BusinessEntities;
using Core.Factories;
using Data.InMemory;
using Data.Repositories;
using Xunit;

namespace UnitTests.Persistence
{
    public class ProductRepositoryTests
    {
        private readonly InMemoryStore _store = new InMemoryStore();

        public ProductRepositoryTests()
        {
            var session = new InMemorySession(_store);
            var repository = new ProductRepository(session);
            repository.Save(NewProduct("Laptop", 1399m, ProductCategories.Hardware));
            repository.Save(NewProduct("Mouse", 25.5m, ProductCategories.Hardware));
            repository.Save(NewProduct("Office suite", 99m, ProductCategories.Software));
            repository.Save(NewProduct("Support plan", 199m, ProductCategories.Service));
            session.Commit();
        }

        [Fact]
        public void Get_NoCriteria_ReturnsEveryProductOrderedByName()
        {
            Assert.Equal(new[] { "Laptop", "Mouse", "Office suite", "Support plan" }, Names(new ProductFilter()));
        }

        [Fact]
        public void Get_ByName_MatchesAFragmentIgnoringCase()
        {
            Assert.Equal(new[] { "Mouse" }, Names(new ProductFilter { Name = " OUS " }));
            Assert.Equal(new[] { "Office suite" }, Names(new ProductFilter { Name = "SUITE" }));
        }

        [Fact]
        public void Get_ByCategoryAndPriceRange_BoundsInclusive()
        {
            Assert.Equal(new[] { "Mouse" }, Names(new ProductFilter { Category = ProductCategories.Hardware, MaxPrice = 1000m }));
            Assert.Equal(new[] { "Office suite", "Support plan" }, Names(new ProductFilter { MinPrice = 99m, MaxPrice = 199m }));
        }

        [Fact]
        public void Get_Pages()
        {
            var repository = new ProductRepository(new InMemorySession(_store));

            Assert.Equal(new[] { "Mouse", "Office suite" }, repository.Get(new ProductFilter(), 1, 2).Select(p => p.Name));
        }

        private string[] Names(ProductFilter filter)
        {
            return new ProductRepository(new InMemorySession(_store)).Get(filter, 0, 50).Select(p => p.Name).ToArray();
        }

        private static Product NewProduct(string name, decimal price, ProductCategories category)
        {
            var product = new IdObjectFactory<Product>().Create(Guid.NewGuid());
            product.SetName(name);
            product.SetPrice(price);
            product.SetCategory(category);
            return product;
        }
    }
}