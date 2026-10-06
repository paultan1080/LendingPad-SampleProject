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
        public void SetTags_Null_ThrowsValidationErrorAndKeepsTags()
        {
            var user = new User();
            user.SetTags(new[] { "A", "C" });

            var exception = Assert.Throws<DomainValidationException>(() => user.SetTags(null));

            Assert.Equal("tags", exception.MemberName);
            Assert.Equal(new[] { "A", "C" }, user.Tags);
        }

        [Fact]
        public void SetTags_Empty_RemovesAllTags()
        {
            var user = new User();
            user.SetTags(new[] { "A", "C" });

            user.SetTags(new string[0]);

            Assert.Empty(user.Tags);
        }

        [Fact]
        public void SetTags_ItsOwnTags_KeepsThem()
        {
            var user = new User();
            user.SetTags(new[] { "A", "C" });

            user.SetTags(user.Tags);

            Assert.Equal(new[] { "A", "C" }, user.Tags);
        }

        [Fact]
        public void SetTags_DropsBlanksTrimsAndRemovesCaseInsensitiveDuplicates()
        {
            var user = new User();

            user.SetTags(new[] { " a ", "A", "", null, "  ", "b" });

            Assert.Equal(new[] { "a", "b" }, user.Tags);
        }

        [Fact]
        public void SetAge_SetsAgeAndLeavesEmailAlone()
        {
            var user = new User();
            user.SetName("John Smith");
            user.SetEmail("john@companya.com");

            user.SetAge(27);

            Assert.Equal(27, user.Age);
            Assert.Equal("john@companya.com", user.Email);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(User.MaxAge + 1)]
        public void SetAge_OutOfRange_ThrowsValidationErrorForAge(int age)
        {
            var exception = Assert.Throws<DomainValidationException>(() => new User().SetAge(age));

            Assert.Equal("age", exception.MemberName);
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