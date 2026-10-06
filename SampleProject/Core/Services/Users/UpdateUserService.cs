using System.Collections.Generic;
using BusinessEntities;
using Common;

namespace Core.Services.Users
{
    [AutoRegister(AutoRegisterTypes.Singleton)]
    public class UpdateUserService : IUpdateUserService
    {
        private const int MonthsPerYear = 12;

        public void Update(User user, string name, string email, UserTypes type, decimal? annualSalary, IEnumerable<string> tags)
        {
            user.SetEmail(email);
            user.SetName(name);
            user.SetType(type);
            // Lifted arithmetic: no annual salary means no monthly salary.
            user.SetMonthlySalary(annualSalary / MonthsPerYear);
            user.SetTags(tags);
        }
    }
}