using BusinessEntities;
using Common.Exceptions;
using Xunit;

namespace UnitTests.Entities
{
    public class UserTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetName_Missing_ThrowsValidationErrorForName(string name)
        {
            var exception = Assert.Throws<DomainValidationException>(() => new User().SetName(name));

            Assert.Equal("name", exception.MemberName);
            Assert.Equal("Name is required.", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetEmail_Missing_ThrowsValidationErrorForEmail(string email)
        {
            var exception = Assert.Throws<DomainValidationException>(() => new User().SetEmail(email));

            Assert.Equal("email", exception.MemberName);
            Assert.Equal("Email is required.", exception.Message);
        }

        [Fact]
        public void SetMonthlySalary_Negative_ThrowsValidationError()
        {
            var exception = Assert.Throws<DomainValidationException>(() => new User().SetMonthlySalary(-1m));

            Assert.Equal("monthlySalary", exception.MemberName);
        }

        [Fact]
        public void SetMonthlySalary_Null_IsAllowed()
        {
            var user = new User();
            user.SetMonthlySalary(5000m);

            user.SetMonthlySalary(null);

            Assert.Null(user.MonthlySalary);
        }

        [Fact]
        public void SetEmail_Valid_KeepsName()
        {
            var user = new User();
            user.SetName("Mark Williams");

            user.SetEmail("mark@companya.com");

            Assert.Equal("mark@companya.com", user.Email);
            Assert.Equal("Mark Williams", user.Name);
        }

        [Fact]
        public void SetType_UndefinedValue_ThrowsValidationErrorForType()
        {
            var exception = Assert.Throws<DomainValidationException>(() => new User().SetType((UserTypes)42));

            Assert.Equal("type", exception.MemberName);
        }
    }
}