using BusinessEntities;
using Core.Services.Users;
using Xunit;

namespace UnitTests.Services
{
    public class UpdateUserServiceTests
    {
        private readonly UpdateUserService _service = new UpdateUserService();

        [Fact]
        public void Update_CopiesScalarFields()
        {
            var user = new User();

            _service.Update(user, "Mark Williams", "mark@companya.com", UserTypes.Admin, 120000m, new[] { "b" });

            Assert.Equal("Mark Williams", user.Name);
            Assert.Equal("mark@companya.com", user.Email);
            Assert.Equal(UserTypes.Admin, user.Type);
        }

        [Fact]
        public void Update_AnnualSalary_StoresMonthlySalary()
        {
            var user = new User();

            _service.Update(user, "John Smith", "john@companya.com", UserTypes.Employee, 72000m, new string[0]);

            Assert.Equal(6000m, user.MonthlySalary);
        }

        [Fact]
        public void Update_NoAnnualSalary_ClearsMonthlySalary()
        {
            var user = new User();
            user.SetMonthlySalary(10000m);

            _service.Update(user, "Mark Williams", "mark@companya.com", UserTypes.Admin, null, new[] { "A" });

            Assert.Null(user.MonthlySalary);
        }
    }
}