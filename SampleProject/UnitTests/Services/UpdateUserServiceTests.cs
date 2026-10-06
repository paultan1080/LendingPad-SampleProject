using System;
using BusinessEntities;
using Common.Exceptions;
using Core.Services.Users;
using Xunit;

namespace UnitTests.Services
{
    public class UpdateUserServiceTests
    {
        private readonly UpdateUserService _service = new UpdateUserService();

        private static UserDetails Mark()
        {
            return new UserDetails
                   {
                       Name = "Mark Williams",
                       Email = "mark@companya.com",
                       Type = UserTypes.Admin,
                       Age = 35,
                       AnnualSalary = 120000m,
                       Tags = new[] { "b" }
                   };
        }

        [Fact]
        public void Update_CopiesScalarFields()
        {
            var user = new User();

            _service.Update(user, Mark());

            Assert.Equal("Mark Williams", user.Name);
            Assert.Equal("mark@companya.com", user.Email);
            Assert.Equal(UserTypes.Admin, user.Type);
            Assert.Equal(35, user.Age);
        }

        [Fact]
        public void Update_AnnualSalary_StoresMonthlySalary()
        {
            var user = new User();
            var details = Mark();
            details.AnnualSalary = 72000m;

            _service.Update(user, details);

            Assert.Equal(6000m, user.MonthlySalary);
        }

        [Fact]
        public void Update_NoAnnualSalary_ClearsMonthlySalary()
        {
            var user = new User();
            user.SetMonthlySalary(10000m);
            var details = Mark();
            details.AnnualSalary = null;

            _service.Update(user, details);

            Assert.Null(user.MonthlySalary);
        }

        [Fact]
        public void Update_NullTags_ThrowsValidationErrorAndKeepsTags()
        {
            var user = new User();
            user.SetTags(new[] { "A", "C", "D" });
            var details = Mark();
            details.Tags = null;

            Assert.Throws<DomainValidationException>(() => _service.Update(user, details));

            Assert.Equal(new[] { "A", "C", "D" }, user.Tags);
        }

        [Fact]
        public void Update_NullArguments_ThrowArgumentNullException()
        {
            Assert.Equal("user", Assert.Throws<ArgumentNullException>(() => _service.Update(null, Mark())).ParamName);
            Assert.Equal("details", Assert.Throws<ArgumentNullException>(() => _service.Update(new User(), null)).ParamName);
        }
    }
}