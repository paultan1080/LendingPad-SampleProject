using BusinessEntities;
using Common.Exceptions;
using Xunit;

namespace UnitTests.Entities
{
    public class ProductTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void SetName_Missing_ThrowsValidationErrorForName(string name)
        {
            var exception = Assert.Throws<DomainValidationException>(() => new Product().SetName(name));

            Assert.Equal("name", exception.MemberName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("  ")]
        public void SetDescription_Blank_RemovesTheDescription(string description)
        {
            var product = new Product();
            product.SetDescription("15-inch laptop");

            product.SetDescription(description);

            Assert.Null(product.Description);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("1499.99")]
        public void SetPrice_ValidAmount_IsAccepted(string price)
        {
            var product = new Product();

            product.SetPrice(decimal.Parse(price));

            Assert.Equal(decimal.Parse(price), product.Price);
        }

        [Theory]
        [InlineData("-0.01")]
        [InlineData("10.999")]
        [InlineData("1000000000.01")]
        public void SetPrice_NegativeOrFractionalCents_ThrowsValidationErrorForPrice(string price)
        {
            var exception = Assert.Throws<DomainValidationException>(() => new Product().SetPrice(decimal.Parse(price)));

            Assert.Equal("price", exception.MemberName);
        }

        [Fact]
        public void SetCategory_UndefinedValue_ThrowsValidationErrorForCategory()
        {
            var exception = Assert.Throws<DomainValidationException>(() => new Product().SetCategory((ProductCategories)42));

            Assert.Equal("category", exception.MemberName);
        }
    }
}